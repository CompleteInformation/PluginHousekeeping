namespace CompleteInformation.Plugins.Housekeeping.Frontend.Web

open Elmish

open CompleteInformation.Plugins.Housekeeping.Api

module Loading =
    type State = {
        lastDone: Map<RoomTask, HistoryMetadata> option
        rooms: Map<RoomId, Room> option
        roomTasks: Set<RoomTask> option
        tasks: Map<TaskId, Task> option
    }

    type Loaded = Map<RoomTask, HistoryMetadata> * Map<RoomId, Room> * Set<RoomTask> * Map<TaskId, Task>

    [<RequireQualifiedAccess>]
    type Intent =
        | None
        | Finish of Loaded

    let getIntent (state: State) =
        match state.lastDone, state.rooms, state.roomTasks, state.tasks with
        | Some lastDone, Some rooms, Some roomTasks, Some tasks -> (lastDone, rooms, roomTasks, tasks) |> Intent.Finish
        | _ -> Intent.None

    type Msg =
        | FetchData
        | SetLastDone of Map<RoomTask, HistoryMetadata>
        | SetRooms of Room list
        | SetTasks of Task list
        | SetRoomTasks of Set<RoomTask>

    let init () : State * Cmd<Msg> =
        let state = {
            lastDone = None
            rooms = None
            roomTasks = None
            tasks = None
        }

        state, Cmd.ofMsg FetchData

    let update housekeepingApi msg (state: State) =
        let state, cmd =
            match msg with
            | FetchData ->
                let cmd =
                    [
                        Cmd.OfAsync.perform housekeepingApi.getRoomTaskLastDone () SetLastDone
                        Cmd.OfAsync.perform housekeepingApi.getRooms () SetRooms
                        Cmd.OfAsync.perform housekeepingApi.getTasks () SetTasks
                        Cmd.OfAsync.perform housekeepingApi.getRoomTasks () SetRoomTasks
                    ]
                    |> Cmd.batch

                state, cmd
            | SetLastDone lastDone -> { state with lastDone = Some lastDone }, Cmd.none
            | SetRooms rooms ->
                let rooms = rooms |> List.map (fun room -> room.id, room) |> Map.ofList
                { state with rooms = Some rooms }, Cmd.none
            | SetTasks tasks ->
                let tasks = tasks |> List.map (fun task -> task.id, task) |> Map.ofList
                { state with tasks = Some tasks }, Cmd.none
            | SetRoomTasks roomTasks ->
                let state =
                    { state with
                        roomTasks = Some roomTasks
                    }

                state, Cmd.none

        state, cmd, getIntent state

    open Feliz
    open Feliz.Bulma

    let render (_: State) (_: Dispatch<Msg>) = Bulma.block [ prop.text "Loading..." ]
