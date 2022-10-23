namespace CompleteInformation.Plugins.Housekeeping.Backend.Web

open CompleteInformation.Base.Backend.Web
open CompleteInformation.Plugins.Housekeeping.Api

[<RequireQualifiedAccess>]
module Persistence =
    module RoomList =
        let private fileName = "rooms.json"

        let save (rooms: Room list) = Persistence.saveJson fileName rooms

        let load () = async {
            let! result = Persistence.loadJson<Room list> fileName

            return
                match result with
                | Persistence.Success rooms -> rooms
                | Persistence.FileNotFound -> []
        }

    module TaskList =
        let private fileName = "tasks.json"

        let save (tasks: Task list) = Persistence.saveJson fileName tasks

        let load () = async {
            let! result = Persistence.loadJson<Task list> fileName

            return
                match result with
                | Persistence.Success tasks -> tasks
                | Persistence.FileNotFound -> []
        }

    module RoomTaskSet =
        let private fileName = "roomTasks.json"

        let save (roomTasks: Set<RoomTask>) = Persistence.saveJson fileName roomTasks

        let load () = async {
            let! result = Persistence.loadJson<Set<RoomTask>> fileName

            return
                match result with
                | Persistence.Success roomTasks -> roomTasks
                | Persistence.FileNotFound -> Set.empty
        }
