module PhotoHelpers.Modes.Organize.View

open Avalonia.Controls
open Avalonia.Layout
open Avalonia.FuncUI.DSL

let view model dispatch =
    StackPanel.create [
        StackPanel.horizontalAlignment HorizontalAlignment.Center
        StackPanel.verticalAlignment VerticalAlignment.Center
        StackPanel.children [
            TextBlock.create [
                TextBlock.text "Organize"
                TextBlock.fontSize 24
            ]

            TextBox.create [
                TextBox.text model.OutputFolderPath
                TextBox.watermark "Output folder"
                TextBox.onTextChanged (Msg.OutputFolderChanged >> dispatch)
            ]

            Button.create [
                Button.content "Start"
                Button.onClick (fun _ -> Msg.Start |> dispatch)
                Button.isEnabled (
                    match model.CurrentState with
                    | Organizing -> false
                    | _ -> true
                )
            ]

            match model.CurrentState with
            | WaitingToStart -> ()
            | Organizing ->
                TextBlock.create [
                    TextBlock.text "Working..."
                ]
            | Finished ->
                TextBlock.create [
                    TextBlock.text "Finished"
                ]
        ]
    ]
