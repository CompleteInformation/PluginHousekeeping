namespace CompleteInformation.Plugins.Housekeeping.Backend.Web

open CompleteInformation.Core
open CompleteInformation.Base.Backend.Web
open CompleteInformation.Plugins.Housekeeping.Api
open FSharp.Core

// For now we work with stub data to design the GUI
type Domain =
    {
        rooms: Room list
        tasks: Task list
        roomTasks: Set<RoomTask>
    }

[<AutoOpen>]
module Data =
    let mutable rooms = []
    let mutable tasks = []
    let mutable roomTasks = Set.empty

    // Load data
    [
        task {
            let! rooms' = Persistence.RoomList.load ()
            rooms <- rooms'
        }
        task {
            let! tasks' = Persistence.TaskList.load ()
            tasks <- tasks'
        }
        task {
            let! roomTasks' = Persistence.RoomTaskSet.load ()
            roomTasks <- roomTasks'
        }
    ]
    |> List.map Async.AwaitTask
    |> Async.Parallel
    |> Async.Ignore
    |> Async.RunSynchronously

[<RequireQualifiedAccess>]
module HousekeepingApi =
    let getRooms () = async { return rooms }

    let putRoom (properties: RoomProperties) =
        async {
            let room = Room.create properties
            rooms <- room :: rooms
            do! Persistence.RoomList.save rooms |> Async.AwaitTask
            return room
        }

    let getTasks () = async { return tasks }

    let putTask (properties: TaskProperties) =
        async {
            let task = Task.create properties
            tasks <- task :: tasks
            do! Persistence.TaskList.save tasks |> Async.AwaitTask
            return task
        }

    let getRoomTasks () = async { return roomTasks }

    let putRoomTask (room: RoomId) (task: TaskId) =
        async {
            let roomTask = { room = room; task = task }
            roomTasks <- Set.add roomTask roomTasks
            do! Persistence.RoomTaskSet.save roomTasks |> Async.AwaitTask
            return roomTask
        }

    let markTaskAsDone roomTask = async { printfn "Done: %A" roomTask }

    let instance: HousekeepingApi =
        {
            getRooms = getRooms
            putRoom = putRoom
            getTasks = getTasks
            putTask = putTask
            getRoomTasks = getRoomTasks
            putRoomTask = putRoomTask
            markTaskAsDone = markTaskAsDone
        }

type Plugin() =
    interface WebserverPlugin with
        member _.getApi devMode routeBuilder =
            Api.build devMode HousekeepingApi.instance routeBuilder

        member _.getMetaData() =
            {
                id = PluginId.create "housekeeping"
                name = "Housekeeping"
            }
