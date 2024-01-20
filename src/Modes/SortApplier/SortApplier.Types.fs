namespace PhotoHelpers.Modes.SortApplier

open PhotoHelpers.Modes.Sorter

[<RequireQualifiedAccess>]
type Msg =
    | Apply of confirmed: bool
    | Deleted

[<RequireQualifiedAccess>]
type DeletionState =
    | None
    | Confirming
    | Deleting
    | Deleted

type Model =
    { SavePath: string
      SortData: SortData array option
      DeletionState: DeletionState }

