module PhotoHelpers.Modes.SortApplier.View

open PhotoHelpers.Modes.SortApplier
open PhotoHelpers.Controls

open Avalonia
open Avalonia.Layout
open Avalonia.Controls
open Avalonia.Media
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Types


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
                TextBlock.text ($"{numberOfPhotosToDelete} photos will be deleted.")
                TextBlock.fontFamily fontFamily
                TextBlock.textAlignment TextAlignment.Center
            ]
        ]
    ]


let images photoPaths =
    WrapPanel.create [
        WrapPanel.margin (Thickness(0, 30, 0, 0))
        WrapPanel.horizontalAlignment HorizontalAlignment.Center
        WrapPanel.orientation Orientation.Horizontal
        WrapPanel.children (
            photoPaths
            |> List.ofArray
            |> List.map (fun imagePath ->
                Border.create [
                    Border.margin (Thickness(5))
                    Border.child (OrientedImage.create imagePath (Some 200))
                ]
            )
        )
    ]


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


let status deletionState =
    let isVisible =
        match deletionState with
        | DeletionState.None -> false
        | _ -> true

    let text =
        match deletionState with
        | DeletionState.None -> ""
        | DeletionState.Confirming -> "Are you sure?"
        | DeletionState.Deleting -> "Deleting..."
        | DeletionState.Deleted -> "Deleted!"

    TextBlock.create [
        TextBlock.isVisible isVisible
        TextBlock.text text
        TextBlock.fontWeight FontWeight.Bold
        TextBlock.fontFamily fontFamily
        TextBlock.textAlignment TextAlignment.Center
        TextBlock.dock Dock.Top
        TextBlock.margin (Thickness(0, 15, 0, 0))
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

        DockPanel.create [
            DockPanel.children [
                heading (photosToDelete |> Array.length)

                status model.DeletionState

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
        |> fun dockPanel ->
            Border.create [
                Border.padding (Thickness(20))
                Border.child dockPanel
            ]
        |> fun border ->
            ScrollViewer.create [
                ScrollViewer.content border
            ]
