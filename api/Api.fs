namespace CompleteInformation.Plugins.Housekeeping.Api

open System

type RoomId = RoomId of string
type Room = { id: RoomId; name: string }

module Room =
    let create (name: string) =
        {
            id = RoomId(name.ToLower())
            name = name
        }

type TaskId = TaskId of string
type Task = { id: TaskId; name: string }

module Task =
    let create (name: string) =
        {
            id = TaskId(name.ToLower())
            name = name
        }

type RoomTask = { room: RoomId; task: TaskId }

type History =
    {
        room: RoomId
        task: TaskId
        time: DateTime
    }

type HousekeepingApi =
    {
        getRooms: unit -> Async<Room list>
        getTasks: unit -> Async<Task list>
        getRoomTasks: unit -> Async<RoomTask list>
        markTaskAsDone: RoomTask -> Async<unit>
    }
