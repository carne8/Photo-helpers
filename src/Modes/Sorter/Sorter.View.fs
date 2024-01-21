module PhotoHelpers.Modes.Sorter.View

open PhotoHelpers.Modes.Sorter

open Avalonia.Layout
open Avalonia.Controls
open Avalonia.Media
open Avalonia.Media.Imaging
open Avalonia.FuncUI.DSL
open System.IO
open SixLabors.ImageSharp
open SixLabors.ImageSharp.Metadata.Profiles.Exif

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

let getImageOrientation path =
    use stream = File.OpenRead path
    let image = Image.Identify stream

    stream.Dispose()

    let exifOrientation =
        image.Metadata.ExifProfile.TryGetValue(ExifTag.Orientation)
        |> function
            | false, _ -> None
            | true, value -> Some value.Value

    match exifOrientation with
    | Some 6us -> 90
    | Some 8us -> -90
    | None
    | Some 1us
    | _ -> 0


let view model _dispatch =
    let bitmap = new Bitmap(model.CurrentPhoto.Path)
    let rotation = model.CurrentPhoto.Path |> getImageOrientation

    Panel.create [
        Panel.children [
            LayoutTransformControl.create [
                LayoutTransformControl.layoutTransform (RotateTransform(rotation))
                LayoutTransformControl.horizontalAlignment HorizontalAlignment.Center
                LayoutTransformControl.verticalAlignment VerticalAlignment.Center

                Image.create [
                    Image.source bitmap
                    Image.bitmapInterpolationMode BitmapInterpolationMode.HighQuality
                ]
                |> LayoutTransformControl.child
            ]

            fileInfoView model.CurrentPhoto
        ]
    ]
