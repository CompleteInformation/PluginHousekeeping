namespace CompleteInformation.Plugins.Housekeeping.Frontend.Web

open CompleteInformation.Plugins.Housekeeping.Api

[<RequireQualifiedAccess>]
type View =
    | Overview
    | Room of RoomId
    | NewRoom of NewRoom.State
    | NewTask of NewTask.State

type Loaded = {
    // Global data
    rooms: Map<RoomId, Room>
    roomTasks: Map<RoomId, TaskId list>
    tasks: Map<TaskId, Task>
    // View-specific data
    view: View
}

type State =
    | Loading of Loading.State
    | Loaded of Loaded
