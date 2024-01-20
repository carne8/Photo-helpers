namespace PhotoHelpers.App

open PhotoHelpers
open Elmish
open Avalonia.Controls

module State =
    let init (window, photosDirectory) =
        { Window = window
          Mode = Mode.Home
          PhotosDirectory =
            photosDirectory
            |> Option.defaultValue "../photos" },
        Cmd.none

    let updateHome homeMsg mainModel =
        match homeMsg with
        | HomeMsg.OpenSorter ->
            let model, cmd = Modes.Sorter.State.init mainModel.PhotosDirectory

            { mainModel with Mode = Mode.Sorter model },
            cmd |> Cmd.map (ModeMsg.Sorter >> Msg.ModeMsg)

        | HomeMsg.OpenSortApplier ->
            let model, cmd = Modes.SortApplier.State.init mainModel.PhotosDirectory

            { mainModel with Mode = Mode.SortApplier model },
            cmd |> Cmd.map (ModeMsg.SortApplier >> Msg.ModeMsg)

        | HomeMsg.OpenConsecutive ->
            let model, cmd = Modes.Consecutive.State.init mainModel.PhotosDirectory

            { mainModel with Mode = Mode.Consecutive model },
            cmd |> Cmd.map (ModeMsg.Consecutive >> Msg.ModeMsg)


    let update msg mainModel =
        match mainModel.Mode, msg with
        // --- Handle HotKeys ---
        // Exit modes
        | _, Msg.KeyPressed Avalonia.Input.Key.Escape ->
            { mainModel with Mode = Mode.Home }, Cmd.none

        // Fullscreen
        | _, Msg.KeyPressed Avalonia.Input.Key.F11 ->
            mainModel.Window.WindowState <-
                match mainModel.Window.WindowState with
                | WindowState.FullScreen -> WindowState.Normal
                | _ -> WindowState.FullScreen

            mainModel, Cmd.none


        // Inject key pressed event
        | Mode.Sorter model, Msg.KeyPressed key ->
            let msgToInject = Modes.Sorter.Msg.KeyPressed key
            let sorterModel, sorterCmd = Modes.Sorter.State.update msgToInject model

            { mainModel with Mode = Mode.Sorter sorterModel },
            sorterCmd |> Cmd.map (ModeMsg.Sorter >> Msg.ModeMsg)


        // --- Manage modes ---
        // Home
        | Mode.Home, Msg.ModeMsg (ModeMsg.Home msg) -> updateHome msg mainModel

        // Sorter
        | Mode.Sorter model, Msg.ModeMsg (ModeMsg.Sorter msg) ->
            let model, cmd = Modes.Sorter.State.update msg model

            { mainModel with Mode = Mode.Sorter model },
            cmd |> Cmd.map (ModeMsg.Sorter >> Msg.ModeMsg)

        // SortApplier
        | Mode.SortApplier model, Msg.ModeMsg (ModeMsg.SortApplier msg) ->
            let model, cmd = Modes.SortApplier.State.update msg model

            { mainModel with Mode = Mode.SortApplier model },
            cmd |> Cmd.map (ModeMsg.SortApplier >> Msg.ModeMsg)

        // Consecutive
        | Mode.Consecutive model, Msg.ModeMsg (ModeMsg.Consecutive msg) ->
            let model, cmd = Modes.Consecutive.State.update msg model

            { mainModel with Mode = Mode.Consecutive model },
            cmd |> Cmd.map (ModeMsg.Consecutive >> Msg.ModeMsg)


        // --- Ignore unexpected states ---
        | _ -> mainModel, Cmd.none