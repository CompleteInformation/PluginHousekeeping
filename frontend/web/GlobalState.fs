namespace CompleteInformation.Plugins.Housekeeping.Frontend.Web

open CompleteInformation.Core
open SimpleOptics

open CompleteInformation.Plugins.Housekeeping.Api

type RoomTasksState = {
    perRoom: Map<RoomId, TaskId list>
    perTask: Map<TaskId, RoomId list>
}

type GlobalState = {
    userId: UserId
    rooms: Map<RoomId, Room>
    tasks: Map<TaskId, Task>
    roomTasks: RoomTasksState
}

// Optics
[<RequireQualifiedAccess>]
module RoomTasksStateOptic =
    let perRoom =
        Lens((fun state -> state.perRoom), (fun state perRoom -> { state with perRoom = perRoom }))

    let perTask =
        Lens((fun state -> state.perTask), (fun state perTask -> { state with perTask = perTask }))

[<RequireQualifiedAccess>]
module GlobalStateOptic =
    let userId =
        Lens((fun state -> state.userId), (fun state userId -> { state with userId = userId }))

    let rooms =
        Lens((fun state -> state.rooms), (fun state rooms -> { state with rooms = rooms }))

    let tasks =
        Lens((fun state -> state.tasks), (fun state tasks -> { state with tasks = tasks }))

    let roomTasks =
        Lens((fun state -> state.roomTasks), (fun state roomTasks -> { state with roomTasks = roomTasks }))

    let roomTasksPerRoom = Optic.compose roomTasks RoomTasksStateOptic.perRoom
    let roomTasksPerTask = Optic.compose roomTasks RoomTasksStateOptic.perTask
