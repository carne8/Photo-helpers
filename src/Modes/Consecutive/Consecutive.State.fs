namespace PhotoHelpers.Modes.Consecutive

open System.IO
open Elmish

module Utils =
    open System.Text.RegularExpressions

    let getPhotoNumber fileName =
        Regex.Match(
            fileName,
            @"dsc_(\d\d\d\d)(?:\.jpg|\.nef)?",
            RegexOptions.IgnoreCase
        )
            .Groups
            .[1]
            .Value
        |> int

    let separateRaws dir =
        dir
        |> Directory.GetFiles
        |> Array.groupBy (fun path -> path.ToLower().Split(".") |> Array.last)

module Cmds =
    let loadPhotosThatBreakContinuity photosDirectory =
        Cmd.ofEffect (fun dispatch ->
            let options = EnumerationOptions(MatchCasing = MatchCasing.CaseInsensitive)
            let photoPaths = Directory.GetFiles(photosDirectory, "*.jpg", options)

            let breakingContinuityCouple =
                photoPaths
                |> Array.pairwise
                |> Array.choose (fun (prev, next) ->
                    let prevNumber = prev |> Path.GetFileNameWithoutExtension |> Utils.getPhotoNumber
                    let nextNumber = next |> Path.GetFileNameWithoutExtension |> Utils.getPhotoNumber

                    match prevNumber + 1 <> nextNumber with
                    | true -> Some (prev, next)
                    | false -> None
                )

            breakingContinuityCouple
            |> Msg.Loaded
            |> dispatch
        )

module State =
    let init photosDirectory =
        { BreakingContinuityPhotoPaths = None },
        Cmds.loadPhotosThatBreakContinuity photosDirectory

    let update msg model =
        match msg with
        | Msg.Loaded photoPaths ->
            { model with BreakingContinuityPhotoPaths = Some photoPaths },
            Cmd.none