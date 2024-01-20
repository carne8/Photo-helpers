module PhotoHelpers.Modes.Consecutive.View

open Avalonia.Layout
open Avalonia.Controls
open Avalonia.FuncUI.DSL
open Avalonia.Media
open Avalonia.Media.Imaging
open System.Diagnostics
open System.IO

let imageCouple (photo1: string) (photo2: string) =
    let openInFileExplorer path =
        Process.Start("explorer.exe", sprintf "/select,\"%s\"" path)
        |> ignore

    let photo1Stream = File.OpenRead(photo1)
    let photo2Stream = File.OpenRead(photo2)

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
        StackPanel.create [
            StackPanel.orientation Orientation.Horizontal
            StackPanel.children [
                Image.create [
                    Image.source (Bitmap.DecodeToHeight(photo1Stream, 200))
                    Image.height 200.0
                    Image.onTapped (fun _ -> photo1 |> openInFileExplorer)
                ]
                Image.create [
                    Image.source (Bitmap.DecodeToHeight(photo2Stream, 200))
                    Image.height 200.0
                    Image.onTapped (fun _ -> photo2 |> openInFileExplorer)
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
