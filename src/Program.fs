module Program

open Avalonia
open Avalonia.Controls.ApplicationLifetimes
open Avalonia.Themes.Fluent
open Avalonia.FuncUI.Hosts
open Avalonia.FuncUI.Elmish
open Elmish
open PhotoHelpers

type MainWindow(directory) as this =
    inherit HostWindow()

    do
        base.Title <- "Photo helpers"
        base.Width <- 800

        let keyDownHandler dispatch =
            this.KeyDown.Subscribe (fun eventArgs ->
                eventArgs.Key
                |> App.Msg.KeyPressed
                |> dispatch
            )

        let keyDownSubscription _model =
            [ ["KeyDown"], keyDownHandler ]

        Program.mkProgram App.State.init App.State.update App.View.view
        |> Program.withHost this
        |> Program.withSubscription keyDownSubscription
        |> Program.runWith (this, directory)

type App(directory) =
    inherit Application()

    override this.Initialize() =
        this.Styles.Add(FluentTheme())
        this.RequestedThemeVariant <- Styling.ThemeVariant.Dark

    override this.OnFrameworkInitializationCompleted() =
        match this.ApplicationLifetime with
        | :? IClassicDesktopStyleApplicationLifetime as desktopLifetime ->
            desktopLifetime.MainWindow <- MainWindow(directory)
        | _ -> ()

[<EntryPoint>]
let main args =
    AppBuilder
        .Configure<App>(fun _ -> args |> Array.tryHead |> App)
        .UsePlatformDetect()
        .UseSkia()
        .StartWithClassicDesktopLifetime(args)
