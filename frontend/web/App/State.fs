namespace CompleteInformation.Plugins.Housekeeping.Frontend.Web

open CompleteInformation.Plugins.Housekeeping.Api

[<RequireQualifiedAccess>]
type View =
    | Overview
    | Room of RoomId
    // Childviews
    | Manager of Manager.State

type Loaded = {
    globalData: GlobalState
    // View-specific data
    view: View
}

type State =
    | Loading of Loading.State
    | Loaded of Loaded
