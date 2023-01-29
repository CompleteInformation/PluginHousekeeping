namespace CompleteInformation.Plugins.Housekeeping.Frontend.Web

open SimpleOptics

open CompleteInformation.Plugins.Housekeeping.Api

[<RequireQualifiedAccess>]
type View =
    | Overview
    // Chosen room, a list of all currently loading task trackings and all tracked tasks
    | Room of RoomId * TaskId list * TaskId list
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

// Optics
[<RequireQualifiedAccess>]
module LoadedOptic =
    let globalData =
        Lens((fun state -> state.globalData), (fun state globalData -> { state with globalData = globalData }))

    let view =
        Lens((fun state -> state.view), (fun state view -> { state with view = view }))

[<RequireQualifiedAccess>]
module StateOptic =
    let loaded =
        Prism(
            (fun state ->
                match state with
                | Loaded x -> Some x
                | _ -> None),
            (fun _ loaded -> Loaded loaded)
        )

    let loadedGlobal = Optic.compose loaded LoadedOptic.globalData
    let loadedGlobalUserId = Optic.compose loadedGlobal GlobalStateOptic.userId
    let loadedGlobalRooms = Optic.compose loadedGlobal GlobalStateOptic.rooms
    let loadedGlobalTasks = Optic.compose loadedGlobal GlobalStateOptic.tasks
    let loadedGlobalRoomTasks = Optic.compose loadedGlobal GlobalStateOptic.roomTasks

    let loadedGlobalRoomTasksPerRoom =
        Optic.compose loadedGlobalRoomTasks RoomTasksStateOptic.perRoom

    let loadedGlobalRoomTasksPerTask =
        Optic.compose loadedGlobalRoomTasks RoomTasksStateOptic.perTask

    let loadedView = Optic.compose loaded LoadedOptic.view
