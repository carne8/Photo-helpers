namespace PhotoHelpers.App

open PhotoHelpers

[<RequireQualifiedAccess>]
type HomeMsg =
    | OpenSorter
    | OpenSortApplier
    | OpenConsecutive
    | OpenOrganize

[<RequireQualifiedAccess>]
type Mode =
    | Home
    | Sorter of Modes.Sorter.Model
    | SortApplier of Modes.SortApplier.Model
    | Consecutive of Modes.Consecutive.Model
    | Organize of Modes.Organize.Model

[<RequireQualifiedAccess>]
type ModeMsg =
    | Home of HomeMsg
    | Sorter of Modes.Sorter.Msg
    | SortApplier of Modes.SortApplier.Msg
    | Consecutive of Modes.Consecutive.Msg
    | Organize of Modes.Organize.Msg

[<RequireQualifiedAccess>]
type Msg =
    | ModeMsg of ModeMsg
    | KeyPressed of Avalonia.Input.Key

type Model =
    { Window: Avalonia.Controls.Window
      Mode: Mode
      PhotosDirectory: string }
