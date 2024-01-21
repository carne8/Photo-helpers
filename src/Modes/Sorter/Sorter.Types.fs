namespace PhotoHelpers.Modes.Sorter

open Thoth.Json.Net

[<RequireQualifiedAccess>]
type Star =
    | One
    | Two
    | Three
    | Four
    | Five

    static member encode star =
        star
        |> function
            | One -> 1
            | Two -> 2
            | Three -> 3
            | Four -> 4
            | Five -> 5
        |> Encode.int

    static member decoder: Decoder<Star> =
        Decode.int |> Decode.andThen (function
            | 1 -> Decode.succeed One
            | 2 -> Decode.succeed Two
            | 3 -> Decode.succeed Three
            | 4 -> Decode.succeed Four
            | 5 -> Decode.succeed Five
            | _ -> Decode.fail "Invalid star value"
        )

type SortData =
    { Path: string
      ToDelete: bool
      Stars: Star option }

    static member encode sortData =
        Encode.object [
            "path", Encode.string sortData.Path
            "toDelete", Encode.bool sortData.ToDelete
            "stars", Encode.option Star.encode sortData.Stars
        ]

    static member decoder: Decoder<SortData> =
        Decode.object (fun get ->
            { Path = get.Required.Field "path" Decode.string
              ToDelete = get.Required.Field "toDelete" Decode.bool
              Stars = get.Optional.Field "stars" Star.decoder }
        )

    static member decodeString rawSortData = Decode.fromString SortData.decoder rawSortData


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
      SortData: SortData array }

