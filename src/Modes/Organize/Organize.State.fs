namespace PhotoHelpers.Modes.Organize

open Elmish
open System
open System.IO
open System.Threading.Tasks
open FsToolkit.ErrorHandling
open SixLabors.ImageSharp
open SixLabors.ImageSharp.Metadata.Profiles.Exif

module Exif =
    let private parseDateTimeExif rawDateTime =
        DateTimeOffset.TryParseExact(
            rawDateTime,
            "yyyy:MM:dd HH:mm:ss",
            null,
            Globalization.DateTimeStyles.None
        )
        |> function
        | false, _ -> None
        | true, dateTime -> Some dateTime

    let getDateTaken (photoPath: string) : DateTimeOffset option =
        let stream = File.OpenRead photoPath
        let image = Image.Identify stream

        stream.Dispose()

        option {
            let! rawDateTime =
                try
                    image.Metadata.ExifProfile.TryGetValue(ExifTag.DateTime)
                    |> function
                    | true, dateTime -> Some dateTime.Value
                    | false, _ -> None
                with _ -> None

            return! rawDateTime |> parseDateTimeExif
        }


module Cmds =
    open Avalonia.Threading
    let private retrieveDateTaken (photoPath: string) =
        let folder = photoPath |> Path.GetDirectoryName
        let fileName = photoPath |> Path.GetFileNameWithoutExtension

        // Check exif for this file or for the JPEG version
        [ photoPath
          sprintf "%s/%s.jpg" folder fileName ]
        |> List.tryPick Exif.getDateTaken
        |> function
        | Some dateTaken -> Ok dateTaken
        | None -> photoPath |> DateTakenNotFound |> Error

    let private getOrganizedPath
        (dateTaken: DateTimeOffset)
        (outputFolderPath: string)
        (photoPath: string)
        =
        let extension =
            photoPath
            |> Path.GetExtension
            |> _.ToLower()

        // Example: 2021/12-December/31
        let dayInTheYear = dateTaken.ToString("yyyy/MM-MMMM/dd")

        let extensionFolder =
            match extension with
            | ".nef" -> "RAW"
            | _ -> ""

        // {BASE_FILE_NAME}-12h-34m{FILE_EXTENSION}
        let format =
            sprintf "'%s'-HH'h'-mm'm''%s'"
                (photoPath
                 |> Path.GetFileNameWithoutExtension
                 |> _.ToUpper())
                extension

        // Example: DSC_1234.jpg -> DSC_1234-12h-34m.jpg
        let newFileName = dateTaken.ToString(format)

        // Example: output_folder/2021/12/31/DSC_1234-12h-34m.jpg
        //          output_folder/2021/12/31/RAW/DSC_1234-12h-34m.nef
        Path.Combine(
            outputFolderPath, // output_folder
            dayInTheYear,     // 2021/12-December/31
            extensionFolder,  // "" or "RAW"
            newFileName       // DSC_1234-12h-34m.jpg
        )

    let private checkIfPhoto (filePath: string) =
        match (filePath |> Path.GetExtension).ToLower() with
        | ".jpg"
        | ".nef" -> Ok ()
        | _ -> filePath |> NotPhotoFile |> Error

    let private organizePhoto outputFolderPath photoPath =
        taskResult {
            do! photoPath |> checkIfPhoto
            let! dateTaken = photoPath |> retrieveDateTaken
            let newPath = photoPath |> getOrganizedPath dateTaken outputFolderPath

            newPath
            |> Path.GetDirectoryName
            |> Directory.CreateDirectory
            |> ignore

            return!
                try
                    File.Copy(photoPath, newPath)
                    |> Ok
                with _ ->
                    photoPath
                    |> FailedToCopyFile
                    |> Error
        }

    let private organizePhotoDir directoryPath outputDirectoryPath =
        directoryPath
        |> Directory.GetFiles
        |> List.ofArray
        |> List.traverseTaskResultA (organizePhoto outputDirectoryPath)
        |> Task.map (function
            | Ok _ -> None
            | Error errors -> Some errors)

    /// The paths need to be full paths
    /// This function starts the organization process as a Cmd
    let startOrganize inputFolderPath outputFolderPath =
        Cmd.ofEffect (fun dispatch ->
            let f =
                Func<Task>(fun () ->
                    organizePhotoDir inputFolderPath outputFolderPath
                    |> Task.map (
                        Option.defaultValue List.empty
                        >> Msg.Finished
                        >> dispatch
                    )
                    :> Task
                )

            Dispatcher.UIThread.InvokeAsync(f, DispatcherPriority.Loaded)
            |> ignore
        )


module State =
    let init inputFolder =

        let outputFolder =
            (inputFolder, "../organized")
            |> Path.Combine
            |> Path.GetFullPath

        { InputFolderPath = inputFolder
          OutputFolderPath = outputFolder
          CurrentState = WaitingToStart },
        Cmd.none

    let update msg model =
        match msg with
        | Msg.OutputFolderChanged outputFolder -> { model with OutputFolderPath = outputFolder }, Cmd.none

        | Msg.Start ->
            let fullOutputFolderPath =
                match model.OutputFolderPath |> Path.IsPathFullyQualified with
                | true -> model.OutputFolderPath
                | false ->
                    (model.InputFolderPath, model.OutputFolderPath)
                    |> Path.Combine
                    |> Path.GetFullPath

            { model with
                CurrentState = Organizing
                OutputFolderPath = fullOutputFolderPath },
            Cmds.startOrganize model.InputFolderPath fullOutputFolderPath

        | Msg.Finished failedPhoto ->
            failedPhoto
            |> List.iter (function
                | NotPhotoFile filePath -> printfn "NotPhotoFile: %s" filePath
                | DateTakenNotFound filePath -> printfn "DateTakenNotFound: %s" filePath
                | FailedToCopyFile filePath -> printfn "FailedToCopyFile: %s" filePath
            )
            { model with CurrentState = Finished }, Cmd.none
