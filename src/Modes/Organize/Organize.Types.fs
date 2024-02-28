namespace PhotoHelpers.Modes.Organize

type OrganizePhotoError =
    | NotPhotoFile of string
    | DateTakenNotFound of string
    | FailedToCopyFile of string

[<RequireQualifiedAccess>]
type Msg =
    | OutputFolderChanged of string
    | Start
    | Finished of failedPhotos: OrganizePhotoError list

type OrganizationProcessState =
    | WaitingToStart
    | Organizing
    | Finished

type Model =
    { InputFolderPath: string
      OutputFolderPath: string
      CurrentState: OrganizationProcessState }
