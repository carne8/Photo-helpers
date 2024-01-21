module PhotoHelpers.Modes.Consecutive.View

open Avalonia.Layout
open Avalonia.Controls
open Avalonia.FuncUI.DSL
open Avalonia.Media
open System.Diagnostics
open System.IO
open PhotoHelpers.Controls

let imageCouple (photo1: string) (photo2: string) =
    let openInFileExplorer path =
        Process.Start("explorer.exe", sprintf "/select,\"%s\"" path)
        |> ignore

    let fontFamily =
        "avares://PhotoHelpers/Assets/Fonts#Space Grotesk"
        |> FontFamily

    let title =
        TextBlock.create [
            sprintf "Continuity broke at %s -> %s"
                (photo1 |> Path.GetFileNameWithoutExtension)
                (photo2 |> Path.GetFileNameWithoutExtension)
            |> fun x -> x.ToUpper()
            |> TextBlock.text

            TextBlock.fontSize 18
            TextBlock.fontWeight FontWeight.Medium
            TextBlock.fontFamily fontFamily
        ]

    let images =
        DockPanel.create [
            DockPanel.children [
                Panel.create [
                    Panel.dock Dock.Left
                    Panel.onTapped (fun _ -> photo1 |> openInFileExplorer)
                    Panel.children [
                        OrientedImage.create photo1 (Some 200)
                    ]
                ]
                Panel.create [
                    Panel.dock Dock.Right
                    Panel.onTapped (fun _ -> photo2 |> openInFileExplorer)
                    Panel.children [
                        OrientedImage.create photo2 (Some 200)
                    ]
                ]
            ]
        ]

    StackPanel.create [
        StackPanel.children [
            title
            images
        ]
    ]

let view model _ =
    ScrollViewer.create [
        ScrollViewer.content (
            StackPanel.create [
                StackPanel.horizontalAlignment HorizontalAlignment.Center
                StackPanel.verticalAlignment VerticalAlignment.Center
                StackPanel.children (
                    match model.BreakingContinuityPhotoPaths with
                    | None -> [ TextBlock.create [ TextBlock.text "Loading..." ] ]
                    | Some [||] -> [ TextBlock.create [ TextBlock.text "No continuity breaks found." ] ]
                    | Some photos ->
                        photos
                        |> List.ofArray
                        |> List.map (fun (photo1, photo2) -> imageCouple photo1 photo2)
                )
            ]
        )
    ]
