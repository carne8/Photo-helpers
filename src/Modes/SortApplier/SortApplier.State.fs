namespace PhotoHelpers.Modes.SortApplier

open PhotoHelpers.Modes.Sorter
open PhotoHelpers.Modes.SortApplier

open System.IO
open Elmish
open Thoth.Json.Net

module Cmds =
    let applySortData savePath sortData =
        Cmd.ofEffect (fun _dispatch ->
            let trashFolder =
                Path.Combine(savePath, "Trash")
                |> Directory.CreateDirectory

            sortData
            |> Array.filter _.ToDelete
            |> Array.iter (fun sortData ->
                let oldPath = sortData.Path |> Path.GetFullPath

                let newPath =
                    Path.Combine(
                        trashFolder.FullName,
                        sortData.Path |> Path.GetFileName
                    )

                File.Move(oldPath, newPath)
            )
        )

module State =
    let init savePath =
        let saveFilePath = Path.Combine(savePath, "mode-sorter.json")

        let sortData =
            match saveFilePath |> File.Exists with
            | false -> None
            | true ->
                saveFilePath
                |> File.ReadAllText
                |> Decode.fromString (Decode.array SortData.decoder)
                |> function
                    | Ok sortData -> Some sortData
                    | Error _ -> None
                |> Option.map (
                    Array.filter (fun sortData ->
                        sortData.ToDelete
                        && sortData.Path |> File.Exists
                    )
                    >> Array.sortBy _.Path
                )

        { SavePath = savePath
          SortData = sortData
          DeletionState = DeletionState.None },
        Cmd.none


    let update msg model =
        match msg, model.SortData with
        | Msg.Apply false, Some _ -> { model with DeletionState = DeletionState.Confirming }, Cmd.none

        | Msg.Apply true, Some sortData ->
            { model with DeletionState = DeletionState.Deleting },
            Cmds.applySortData model.SavePath sortData

        | Msg.Deleted, _ -> { model with DeletionState = DeletionState.Deleted }, Cmd.none

        | _ -> model, Cmd.none
