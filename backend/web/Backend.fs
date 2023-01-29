namespace CompleteInformation.Plugins.Housekeeping.Backend.Web

open CompleteInformation.Core
open CompleteInformation.Base.Backend.Web
open CompleteInformation.Plugins.Housekeeping.Api
open FSharp.Core
open System

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
        // We also load the last time a task was done
        let! history =
            roomTasks
            |> Seq.map (fun rt -> async {
                let! last = Persistence.History.getLast rt
                return (rt, last)
            })
            |> Async.Parallel

        let historyMap =
            Seq.choose (fun (rt, last) -> Option.map (fun last -> rt, last) last) history
            |> Map.ofSeq

        log "Loaded roomTask data."
        return roomTasks, historyMap
    }

    log "Loading data..."

    let mutable rooms = []
    let mutable tasks = []
    let mutable roomTasks = Set.empty
    let mutable history = Map.empty

    // Load data
    async {
        let! rooms = loadRooms ()
        let! tasks = loadTasks ()
        let! (roomTasks, history) = loadRoomTasks ()

        return rooms, tasks, roomTasks, history
    }
    |> Async.RunSynchronously
    |> fun (rooms', tasks', roomTasks', history') ->
        rooms <- rooms'
        tasks <- tasks'
        roomTasks <- roomTasks'
        history <- history'

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

    let getRoomTaskLastDone () = async { return history }

    let putRoomTask (roomTask: RoomTask) = async {
        roomTasks <- Set.add roomTask roomTasks
        do! Persistence.RoomTaskSet.save roomTasks
    }

    let deleteRoomTask (roomTask: RoomTask) = async {
        roomTasks <- Set.remove roomTask roomTasks
        do! Persistence.RoomTaskSet.save roomTasks
    }

    let trackRoomTaskDone userId roomTask = async {
        let metadata = { time = DateTime.Now; user = userId }

        let entry = {
            roomTask = roomTask
            metadata = metadata
        }

        history <- Map.add roomTask metadata history
        do! Persistence.History.append entry
    }

    let instance: HousekeepingApi = {
        getRooms = getRooms
        putRoom = putRoom
        getTasks = getTasks
        putTask = putTask
        getRoomTasks = getRoomTasks
        getRoomTaskLastDone = getRoomTaskLastDone
        putRoomTask = putRoomTask
        deleteRoomTask = deleteRoomTask
        trackRoomTaskDone = trackRoomTaskDone
    }

type Plugin() =
    interface WebserverPlugin with
        member _.getApi devMode routeBuilder =
            Api.build devMode HousekeepingApi.instance routeBuilder

        member _.getMetaData() = {
            id = Const.moduleName |> PluginId.create
            name = "Housekeeping"
        }
