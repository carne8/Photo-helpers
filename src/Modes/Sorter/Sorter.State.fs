namespace PhotoHelpers.Modes.Sorter

open System
open System.IO

open Elmish
open Avalonia.Input
open Thoth.Json.Net

module Cmds =
    let saveSortData savePath sortData =
        Cmd.ofEffect (fun _dispatch ->
            let rawSortData = sortData |> SortData.encode
            let saveFilePath = Path.Combine(savePath, "mode-sorter.json")

            match File.Exists saveFilePath with
            | false -> File.Create(saveFilePath) |> ignore
            | true -> ()

            File.WriteAllTextAsync(saveFilePath, rawSortData)
            |> ignore
        )

    let updateStarOnPhoto currentPhoto newStar =
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

    let toggleTrashStateOnPhoto currentPhoto =
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
    let init photosDirectory =
        let savedSortDataOpt =
            let saveFilePath = Path.Combine(photosDirectory, "mode-sorter.json")

            match saveFilePath |> File.Exists with
            | false -> None
            | true ->
                saveFilePath
                |> File.ReadAllText
                |> SortData.decode
                |> Result.toOption

        let photoPaths =
            photosDirectory
            |> Directory.GetFiles
            |> Array.filter (fun path ->
                path.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase)
            )

        { SavePath = photosDirectory
          PhotoPaths = photoPaths
          CurrentPhoto = { Path = photoPaths |> Array.head
                           Index = 0
                           SortData = None }
          SortedPhotos = savedSortDataOpt
                         |> Option.defaultValue Array.empty },
        Cmd.none

    let loadPhoto model offset =
        let newIdx =
            Math.Clamp(
                model.CurrentPhoto.Index + offset,
                0,
                model.PhotoPaths.Length - 1
            )

        let newPhotoPath = model.PhotoPaths |> Array.item newIdx

        let sortData =
            model.SortedPhotos
            |> Array.tryFind (fun sp -> sp.Path = newPhotoPath)

        let newPhoto =
            { Path = newPhotoPath
              Index = newIdx
              SortData = sortData }

        { model with CurrentPhoto = newPhoto }


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

        | Msg.NextPhoto -> loadPhoto model +1, Cmd.none
        | Msg.PrevPhoto -> loadPhoto model -1, Cmd.none

        | Msg.SortPhoto sortedPhoto ->
            let newSortData =
                model.SortedPhotos
                |> Array.filter (fun sp -> sp.Path <> sortedPhoto.Path)
                |> Array.append [| sortedPhoto |]

            let newCurrentPhoto =
                match model.CurrentPhoto.Path = sortedPhoto.Path with
                | true -> { model.CurrentPhoto with SortData = Some sortedPhoto }
                | false -> model.CurrentPhoto

            { model with
                CurrentPhoto = newCurrentPhoto
                SortedPhotos = newSortData },
            Cmds.saveSortData model.SavePath newSortData