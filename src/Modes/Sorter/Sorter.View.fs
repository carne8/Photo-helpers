module PhotoHelpers.Modes.Sorter.View

open PhotoHelpers.Modes.Sorter
open PhotoHelpers.Controls

open Avalonia.Layout
open Avalonia.Controls
open Avalonia.Media
open Avalonia.FuncUI.DSL
open System.IO

let fileInfoView photo =
    let fontFamily =
        "avares://PhotoHelpers/Assets/Fonts#Space Grotesk"
        |> FontFamily

    StackPanel.create [
        StackPanel.children [
            TextBlock.create [
                TextBlock.text (photo.Path |> Path.GetFileNameWithoutExtension)
                TextBlock.fontWeight FontWeight.Medium
                TextBlock.fontFamily fontFamily
            ]

            match photo.SortData with
            | None -> ()
            | Some sortData ->
                match sortData.Stars with
                | None -> ()
                | Some stars ->
                    StackPanel.create [
                        StackPanel.orientation Orientation.Horizontal
                        StackPanel.children [
                            TextBlock.create [
                                TextBlock.text "Stars: "
                                TextBlock.fontFamily fontFamily
                            ]

                            TextBlock.create [
                                TextBlock.text (string stars)
                                TextBlock.fontWeight FontWeight.Bold
                                TextBlock.fontFamily fontFamily
                            ]
                        ]
                    ]

                match sortData.ToDelete with
                | false -> ()
                | true ->
                    TextBlock.create [
                        TextBlock.text "In trash"
                        TextBlock.fontWeight FontWeight.Bold
                        TextBlock.fontFamily fontFamily
                    ]
        ]
    ]


let view model _dispatch =
    Panel.create [
        Panel.children [
            OrientedImage.create model.CurrentPhoto.Path None
            fileInfoView model.CurrentPhoto
        ]
    ]
