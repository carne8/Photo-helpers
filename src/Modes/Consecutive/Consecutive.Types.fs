namespace PhotoHelpers.Modes.Consecutive

// type

[<RequireQualifiedAccess>]
type Msg = | Loaded of (string * string) array

type Model =
    { BreakingContinuityPhotoPaths: (string * string) array option }