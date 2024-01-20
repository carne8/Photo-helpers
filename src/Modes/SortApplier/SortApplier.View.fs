module PhotoHelpers.Modes.SortApplier.View

open PhotoHelpers.Modes.SortApplier

open Avalonia
open Avalonia.Layout
open Avalonia.Controls
open Avalonia.Media
open Avalonia.Media.Imaging
open Avalonia.FuncUI
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Types
open System.IO

let fontFamily =
    "avares://PhotoHelpers/Assets/Fonts#Space Grotesk"
    |> FontFamily

let heading numberOfPhotosToDelete =
    StackPanel.create [
        StackPanel.horizontalAlignment HorizontalAlignment.Center
        StackPanel.dock Dock.Top
        StackPanel.children [
            TextBlock.create [
                TextBlock.text "Sort Applier"
                TextBlock.fontSize 24
                TextBlock.fontWeight FontWeight.Bold
                TextBlock.fontFamily fontFamily
                TextBlock.textAlignment TextAlignment.Center
            ]

            TextBlock.create [
                TextBlock.text "This mode applies the sorting data from the Sorter mode to your photos."
                TextBlock.fontFamily fontFamily
                TextBlock.textAlignment TextAlignment.Center
            ]

            TextBlock.create [
                TextBlock.text (sprintf "%i photos will be deleted." numberOfPhotosToDelete)
                TextBlock.fontFamily fontFamily
                TextBlock.textAlignment TextAlignment.Center
            ]
        ]
    ]

let images photoPaths =
    Component.create("Images", fun ctx ->
        let bitmaps =
            photoPaths
            |> List.ofArray
            |> List.map (fun photoPath ->
                let stream = photoPath |> File.OpenRead
                stream, Bitmap.DecodeToHeight(stream, 200)
            )

        ctx.control.Unloaded.Add(fun _ ->
            bitmaps |> List.iter (fun (stream, bitmap) ->
                bitmap.Dispose()
                stream.Dispose()
            )
        )

        WrapPanel.create [
            WrapPanel.margin (Thickness(0, 30, 0, 0))
            WrapPanel.horizontalAlignment HorizontalAlignment.Center
            WrapPanel.orientation Orientation.Horizontal
            WrapPanel.children (
                bitmaps |> List.map (fun (_, bitmap) ->
                    Image.create [
                        Image.source bitmap
                        Image.height 200.
                        Image.margin (Thickness(5))
                    ]
                )
            )
        ]
    )

let button isConfirm dispatch =
    Button.create [
        Button.content (
            TextBlock.create [
                TextBlock.text (isConfirm |> function
                    | false -> "Delete"
                    | true -> "I'm sure")
                TextBlock.fontFamily fontFamily
            ]
        )

        Button.margin (Thickness(0, 15))
        Button.dock Dock.Top
        Button.horizontalAlignment HorizontalAlignment.Center
        Button.onTapped(
            (fun _ -> isConfirm |> Msg.Apply |> dispatch),
            OnChangeOf isConfirm
        )
    ]

let view model _dispatch =
    match model.SortData with
    | None ->
        TextBlock.create [
            TextBlock.text "No sort data found."
            TextBlock.fontSize 18
            TextBlock.fontFamily fontFamily

            TextBlock.horizontalAlignment HorizontalAlignment.Center
            TextBlock.verticalAlignment VerticalAlignment.Center
        ] :> IView

    | Some photosToDelete ->
        Border.create [
            Border.padding (Thickness(20))

            DockPanel.create [
                DockPanel.children [
                    heading (photosToDelete |> Array.length)

                    match model.DeletionState with
                    | DeletionState.None -> button false _dispatch
                    | DeletionState.Confirming -> button true _dispatch
                    | DeletionState.Deleting -> ()
                    | DeletionState.Deleted -> ()

                    match model.DeletionState with
                    | DeletionState.None
                    | DeletionState.Confirming -> images (photosToDelete |> Array.map _.Path)
                    | _ -> ()
                ]
            ]
            |> Border.child
        ]