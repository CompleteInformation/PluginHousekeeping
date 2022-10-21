namespace CompleteInformation.Plugins.Housekeeping.Backend.Web

open System.IO

open CompleteInformation.Base.Backend.Web
open CompleteInformation.Plugins.Housekeeping.Api

[<RequireQualifiedAccess>]
module Persistence =
    module RoomList =
        let private fileName = "rooms.json"

        let save (rooms: Room list) = Persistence.saveJson fileName rooms

        let load () =
            task {
                if File.Exists fileName then
                    return! Persistence.loadJson<Room list> fileName
                else
                    return []
            }

    module TaskList =
        let private fileName = "tasks.json"

        let save (tasks: Task list) = Persistence.saveJson fileName tasks

        let load () =
            task {
                if File.Exists fileName then
                    return! Persistence.loadJson<Task list> fileName
                else
                    return []
            }

    module RoomTaskSet =
        let private fileName = "roomTasks.json"

        let save (roomTasks: Set<RoomTask>) = Persistence.saveJson fileName roomTasks

        let load () =
            task {
                if File.Exists fileName then
                    return! Persistence.loadJson<Set<RoomTask>> fileName
                else
                    return Set.empty
            }
