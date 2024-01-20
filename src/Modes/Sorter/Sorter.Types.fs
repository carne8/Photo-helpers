namespace PhotoHelpers.Modes.Sorter

open Thoth.Json.Net

[<RequireQualifiedAccess>]
type Star =
    | One
    | Two
    | Three
    | Four
    | Five

type SortData =
    { Path: string
      ToDelete: bool
      Stars: Star option }

    static member private encoder = Encode.Auto.generateEncoderCached<SortData array> CamelCase
    static member private decoder = Decode.Auto.generateDecoderCached<SortData array> CamelCase

    static member encode sortData = SortData.encoder sortData |> Encode.toString 2
    static member decode rawSortData = Decode.fromString SortData.decoder rawSortData


[<RequireQualifiedAccess>]
type Msg =
    | NextPhoto
    | PrevPhoto
    | KeyPressed of Avalonia.Input.Key
    | SortPhoto of SortData

type PhotoInfo =
    { Path: string
      Index: int
      SortData: SortData option }

type Model =
    { SavePath: string
      PhotoPaths: string array
      CurrentPhoto: PhotoInfo
      SortedPhotos: SortData array }

