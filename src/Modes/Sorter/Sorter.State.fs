namespace PhotoHelpers.Modes.Sorter

open System
open System.IO

open Elmish
open Avalonia.Input
open Thoth.Json.Net

module Cmds =
    let saveSortData savePath sortData =
        Cmd.ofEffect (fun _dispatch ->
            let rawSortData =
                sortData
                |> Array.map SortData.encode
                |> Encode.array
                |> Encode.toString 2

            let saveFilePath = Path.Combine(savePath, "mode-sorter.json")

            match File.Exists saveFilePath with
            | false ->
                let stream = File.Create(saveFilePath)
                stream.Close()
            | true -> ()

            File.WriteAllTextAsync(saveFilePath, rawSortData) |> ignore
        )

    let updateStarOnPhoto (currentPhoto: PhotoInfo) newStar =
        Cmd.ofEffect (fun dispatch ->
            let sortedPhoto =
                match currentPhoto.SortData with
                | None -> { Path = currentPhoto.Path
                            ToDelete = false
                            Stars = Some newStar }
                | Some sortedPhoto ->
                    match sortedPhoto.Stars with
                    | Some star when star = newStar -> { sortedPhoto with Stars = None }
                    | _ -> { sortedPhoto with Stars = Some newStar }

            sortedPhoto
            |> Msg.SortPhoto
            |> dispatch
        )

    let toggleTrashStateOnPhoto (currentPhoto: PhotoInfo) =
        Cmd.ofEffect (fun dispatch ->
            let sortedPhoto =
                match currentPhoto.SortData with
                | None -> { Path = currentPhoto.Path
                            ToDelete = true
                            Stars = None }
                | Some sortedPhoto ->
                    { sortedPhoto with ToDelete = not sortedPhoto.ToDelete }

            sortedPhoto
            |> Msg.SortPhoto
            |> dispatch
        )

module State =
    let loadNextPhoto offset model =
        let newIdx =
            Math.Clamp(
                model.CurrentPhoto.Index + offset,
                0,
                model.PhotoPaths.Length - 1
            )

        let newPhotoPath = model.PhotoPaths |> Array.item newIdx

        let sortData =
            model.SortData
            |> Array.tryFind (fun sp -> sp.Path = newPhotoPath)

        let newPhoto =
            { Path = newPhotoPath
              Index = newIdx
              SortData = sortData }

        { model with CurrentPhoto = newPhoto }

    let init photosDirectory =
        let loadPhotoPaths model =
            let photoPaths =
                photosDirectory
                |> Directory.GetFiles
                |> Array.filter (fun path ->
                    path.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase)
                )

            { model with PhotoPaths = photoPaths }

        let loadSavedSortData model : Model =
            let saveFilePath = Path.Combine(photosDirectory, "mode-sorter.json")

            match saveFilePath |> File.Exists with
            | false -> { model with SortData = Array.empty }
            | true ->
                let sortData =
                    saveFilePath
                    |> File.ReadAllText
                    |> Decode.fromString (Decode.array SortData.decoder)
                    |> Result.toOption
                    |> Option.defaultValue Array.empty

                { model with SortData = sortData }

        let dummyModel =
            { SavePath = photosDirectory
              PhotoPaths = Array.empty
              CurrentPhoto = { Path = ""; Index = 0; SortData = None }
              SortData = Array.empty }

        dummyModel
        |> loadPhotoPaths
        |> loadSavedSortData
        |> loadNextPhoto 0,
        Cmd.none

    let update msg model =
        match msg with
        | Msg.KeyPressed Key.Right -> model, Cmd.ofMsg Msg.NextPhoto
        | Msg.KeyPressed Key.Left -> model, Cmd.ofMsg Msg.PrevPhoto

        | Msg.KeyPressed Key.D1 -> model, Cmds.updateStarOnPhoto model.CurrentPhoto Star.One
        | Msg.KeyPressed Key.D2 -> model, Cmds.updateStarOnPhoto model.CurrentPhoto Star.Two
        | Msg.KeyPressed Key.D3 -> model, Cmds.updateStarOnPhoto model.CurrentPhoto Star.Three
        | Msg.KeyPressed Key.D4 -> model, Cmds.updateStarOnPhoto model.CurrentPhoto Star.Four
        | Msg.KeyPressed Key.D5 -> model, Cmds.updateStarOnPhoto model.CurrentPhoto Star.Five

        | Msg.KeyPressed Key.Q -> model, Cmds.toggleTrashStateOnPhoto model.CurrentPhoto

        | Msg.KeyPressed _ -> model, Cmd.none

        | Msg.NextPhoto -> loadNextPhoto +1 model, Cmd.none
        | Msg.PrevPhoto -> loadNextPhoto -1 model, Cmd.none

        | Msg.SortPhoto sortedPhoto ->
            let newSortData =
                model.SortData
                |> Array.filter (fun sp -> sp.Path <> sortedPhoto.Path)
                |> Array.append [| sortedPhoto |]

            let newCurrentPhoto =
                match model.CurrentPhoto.Path = sortedPhoto.Path with
                | true -> { model.CurrentPhoto with SortData = Some sortedPhoto }
                | false -> model.CurrentPhoto

            { model with
                CurrentPhoto = newCurrentPhoto
                SortData = newSortData },
            Cmds.saveSortData model.SavePath newSortData
