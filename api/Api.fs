namespace CompleteInformation.Plugins.Housekeeping.Api

open System

type RoomId = RoomId of Guid
type RoomProperties = { name: string }

type Room =
    {
        id: RoomId
        properties: RoomProperties
    }

module Room =
    let create properties =
        {
            id = RoomId(Guid.NewGuid())
            properties = properties
        }

type TaskId = TaskId of Guid
type TaskProperties = { name: string }

type Task =
    {
        id: TaskId
        properties: TaskProperties
    }

module Task =
    let create properties =
        {
            id = TaskId(Guid.NewGuid())
            properties = properties
        }

type RoomTask = { room: RoomId; task: TaskId }

type HistoryEntry =
    {
        room: RoomId
        task: TaskId
        time: DateTime
    }

type History = HistoryEntry seq

type HousekeepingApi =
    {
        getRooms: unit -> Async<Room list>
        putRoom: RoomProperties -> Async<Room>
        getTasks: unit -> Async<Task list>
        putTask: TaskProperties -> Async<Task>
        getRoomTasks: unit -> Async<Set<RoomTask>>
        putRoomTask: RoomId -> TaskId -> Async<RoomTask>
        markTaskAsDone: RoomTask -> Async<unit>
    }
