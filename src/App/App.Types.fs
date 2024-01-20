namespace PhotoHelpers.App

open PhotoHelpers

[<RequireQualifiedAccess>]
type HomeMsg =
    | OpenSorter
    | OpenSortApplier
    | OpenConsecutive

[<RequireQualifiedAccess>]
type Mode =
    | Home
    | Sorter of Modes.Sorter.Model
    | SortApplier of Modes.SortApplier.Model
    | Consecutive of Modes.Consecutive.Model

[<RequireQualifiedAccess>]
type ModeMsg =
    | Home of HomeMsg
    | Sorter of Modes.Sorter.Msg
    | SortApplier of Modes.SortApplier.Msg
    | Consecutive of Modes.Consecutive.Msg

[<RequireQualifiedAccess>]
type Msg =
    | ModeMsg of ModeMsg
    | KeyPressed of Avalonia.Input.Key

type Model =
    { Window: Avalonia.Controls.Window
      Mode: Mode
      PhotosDirectory: string }