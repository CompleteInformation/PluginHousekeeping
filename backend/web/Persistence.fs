namespace CompleteInformation.Plugins.Housekeeping.Backend.Web

open CompleteInformation.Base.Backend.Web
open CompleteInformation.Plugins.Housekeeping.Api

[<RequireQualifiedAccess>]
module Persistence =
    module JsonL =
        let inline append<'a> = Persistence.JsonL.append<'a> Const.moduleName
        let inline load<'a> = Persistence.JsonL.load<'a> Const.moduleName

        let inline save<'a> file =
            Persistence.JsonL.save<'a> Const.moduleName file

    module RoomList =
        let private fileName = "rooms.jsonl"

        let save (rooms: Room list) = JsonL.save fileName rooms

        let load () = async {
            let! result = JsonL.load<Room> fileName

            return
                match result with
                | Persistence.Success rooms -> rooms |> List.ofSeq
                | Persistence.FileNotFound -> []
        }

    module TaskList =
        let private fileName = "tasks.jsonl"

        let save (tasks: Task list) = async { do! tasks |> List.toSeq |> JsonL.save fileName }

        let load () = async {
            let! result = JsonL.load<Task> fileName

            return
                match result with
                | Persistence.Success tasks -> tasks |> List.ofSeq
                | Persistence.FileNotFound -> []
        }

    module RoomTaskSet =
        let private fileName = "roomTasks.jsonl"

        let save (roomTasks: Set<RoomTask>) = async { do! roomTasks |> Set.toSeq |> JsonL.save fileName }

        let load () = async {
            let! result = JsonL.load<RoomTask> fileName

            return
                match result with
                | Persistence.Success roomTasks -> roomTasks |> Set.ofSeq
                | Persistence.FileNotFound -> Set.empty
        }

    module History =
        let private getFileName roomId taskId =
            $"history/{RoomId.toString roomId}_{TaskId.toString taskId}.jsonl"

        let append (entry: HistoryEntry) = async {
            let fileName = getFileName entry.roomTask.room entry.roomTask.task

            do! JsonL.append fileName entry.metadata
        }
