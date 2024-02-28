namespace PhotoHelpers.Controls

open Avalonia.Controls
open Avalonia.Layout
open Avalonia.Media
open Avalonia.Media.Imaging
open Avalonia.FuncUI.DSL

open System.IO
open SixLabors.ImageSharp
open SixLabors.ImageSharp.Metadata.Profiles.Exif


[<RequireQualifiedAccess>]
module OrientedImage =

    let private getImageOrientation path =
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


    let create (imagePath: string) height =
        let bitmap =
            match height with
            | None -> new Bitmap(imagePath)
            | Some height ->
                let stream = File.OpenRead imagePath
                let bitmap = Bitmap.DecodeToHeight(stream, height)

                stream.Dispose()
                bitmap

        let rotation = imagePath |> getImageOrientation

        LayoutTransformControl.create [
            match height with
            | None -> ()
            | Some height -> LayoutTransformControl.height height

            LayoutTransformControl.layoutTransform (RotateTransform(rotation))
            LayoutTransformControl.horizontalAlignment HorizontalAlignment.Center
            LayoutTransformControl.verticalAlignment VerticalAlignment.Center

            Image.create [
                Image.source bitmap
                Image.bitmapInterpolationMode BitmapInterpolationMode.HighQuality
            ]
            |> LayoutTransformControl.child
        ]
