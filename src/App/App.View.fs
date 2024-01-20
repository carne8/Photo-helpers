module PhotoHelpers.App.View

open PhotoHelpers

open Avalonia.Layout
open Avalonia.Controls
open Avalonia.Media
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Types


// Home
let homeView _ dispatch =
    let modes =
        [ "Sorter", HomeMsg.OpenSorter
          "Sort Applier", HomeMsg.OpenSortApplier
          "Consecutive", HomeMsg.OpenConsecutive ]

    let fontFamily =
        "avares://PhotoHelpers/Assets/Fonts#Space Grotesk"
        |> FontFamily

    StackPanel.create [
        StackPanel.horizontalAlignment HorizontalAlignment.Center
        StackPanel.verticalAlignment VerticalAlignment.Center
        StackPanel.children [
            TextBlock.create [
                TextBlock.text ("Photo helpers".ToUpper())
                TextBlock.fontSize 24
                TextBlock.fontWeight FontWeight.Medium
                TextBlock.fontFamily fontFamily
            ]
            yield! modes |> List.map (fun (modeName, onClickMsg) ->
                Button.create [
                    Button.horizontalAlignment HorizontalAlignment.Stretch
                    Button.content modeName
                    Button.onClick (fun _ -> onClickMsg |> dispatch)
                ] :> IView
            )
        ]
    ]
    :> IView


// Root view
let view model dispatch =
    match model.Mode with
    | Mode.Home -> homeView model (ModeMsg.Home >> Msg.ModeMsg >> dispatch)
    | Mode.Sorter model -> Modes.Sorter.View.view model (ModeMsg.Sorter >> Msg.ModeMsg >> dispatch)
    | Mode.SortApplier model -> Modes.SortApplier.View.view model (ModeMsg.SortApplier >> Msg.ModeMsg >> dispatch)
    | Mode.Consecutive model -> Modes.Consecutive.View.view model (ModeMsg.Consecutive >> Msg.ModeMsg >> dispatch)