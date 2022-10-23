namespace CompleteInformation.Plugins.Housekeeping.Backend.Web

open CompleteInformation.Core
open CompleteInformation.Base.Backend.Web
open CompleteInformation.Plugins.Housekeeping.Api
open FSharp.Core

// For now we work with stub data to design the GUI
type Domain = {
    rooms: Room list
    tasks: Task list
    roomTasks: Set<RoomTask>
}

// Helper for logging only in Debug mode
// TODO: Move into Base
[<AutoOpen>]
module Logger =
    let log msg =
#if DEBUG
        printfn "[Plugin - Housekeeping] %s" msg
#else
        ()
#endif

[<AutoOpen>]
module Data =
    let mutable rooms = []
    let mutable tasks = []
    let mutable roomTasks = Set.empty

    let loadRooms () = async {
        log "Loading room data..."
        let! rooms = Persistence.RoomList.load ()
        log "Loaded room data."
        return rooms
    }

    let loadTasks () = async {
        log "Loading task data..."
        let! tasks = Persistence.TaskList.load ()
        log "Loaded task data."
        return tasks
    }

    let loadRoomTasks () = async {
        log "Loading roomTask data..."
        let! roomTasks = Persistence.RoomTaskSet.load ()
        log "Loaded roomTask data."
        return roomTasks
    }

    log "Loading data..."

    // Load data
    async {
        let! rooms = loadRooms ()
        let! tasks = loadTasks ()
        let! roomTasks = loadRoomTasks ()

        return rooms, tasks, roomTasks
    }
    |> Async.RunSynchronously
    |> fun (rooms', tasks', roomTasks') ->
        rooms <- rooms'
        tasks <- tasks'
        roomTasks <- roomTasks'

    log "Loaded data."

[<RequireQualifiedAccess>]
module HousekeepingApi =
    let getRooms () = async { return rooms }

    let putRoom (properties: RoomProperties) = async {
        let room = Room.create properties
        rooms <- room :: rooms
        do! Persistence.RoomList.save rooms
        return room
    }

    let getTasks () = async { return tasks }

    let putTask (properties: TaskProperties) = async {
        let task = Task.create properties
        tasks <- task :: tasks
        do! Persistence.TaskList.save tasks
        return task
    }

    let getRoomTasks () = async { return roomTasks }

    let putRoomTask (roomTask: RoomTask) = async {
        roomTasks <- Set.add roomTask roomTasks
        do! Persistence.RoomTaskSet.save roomTasks
    }

    let deleteRoomTask (roomTask: RoomTask) = async {
        roomTasks <- Set.remove roomTask roomTasks
        do! Persistence.RoomTaskSet.save roomTasks
    }

    let markTaskAsDone roomTask = async { printfn "Done: %A" roomTask }

    let instance: HousekeepingApi = {
        getRooms = getRooms
        putRoom = putRoom
        getTasks = getTasks
        putTask = putTask
        getRoomTasks = getRoomTasks
        putRoomTask = putRoomTask
        deleteRoomTask = deleteRoomTask
        markTaskAsDone = markTaskAsDone
    }

type Plugin() =
    interface WebserverPlugin with
        member _.getApi devMode routeBuilder =
            Api.build devMode HousekeepingApi.instance routeBuilder

        member _.getMetaData() = {
            id = PluginId.create "housekeeping"
            name = "Housekeeping"
        }
