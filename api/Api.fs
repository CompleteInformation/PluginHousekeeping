namespace CompleteInformation.Plugins.Housekeeping.Api

open CompleteInformation.Core
open System

type RoomId = RoomId of Guid

module RoomId =
    let toString (RoomId id) = id.ToString()

type RoomProperties = { name: string }

type Room = {
    id: RoomId
    properties: RoomProperties
}

module Room =
    let create properties = {
        id = RoomId(Guid.NewGuid())
        properties = properties
    }

type TaskId = TaskId of Guid

module TaskId =
    let toString (TaskId id) = id.ToString()

type TaskProperties = { name: string }

type Task = {
    id: TaskId
    properties: TaskProperties
}

module Task =
    let create properties = {
        id = TaskId(Guid.NewGuid())
        properties = properties
    }

type RoomTask = { room: RoomId; task: TaskId }

type HistoryMetadata = { time: DateTime; user: UserId }

type HistoryEntry = {
    roomTask: RoomTask
    metadata: HistoryMetadata
}

type History = HistoryEntry seq

type HousekeepingApi = {
    getRooms: unit -> Async<Room list>
    putRoom: RoomProperties -> Async<Room>
    getTasks: unit -> Async<(Task) list>
    putTask: TaskProperties -> Async<Task>
    getRoomTasks: unit -> Async<Set<RoomTask>>
    putRoomTask: RoomTask -> Async<unit>
    getRoomTaskLastDone: unit -> Async<Map<RoomTask, HistoryMetadata>>
    deleteRoomTask: RoomTask -> Async<unit>
    trackRoomTaskDone: UserId -> RoomTask -> Async<unit>
}
