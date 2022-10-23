namespace CompleteInformation.Plugins.Housekeeping.Frontend.Web

open Elmish

open CompleteInformation.Plugins.Housekeeping.Api

module Loading =
    type State = {
        rooms: Map<RoomId, Room> option
        roomTasks: Map<RoomId, TaskId list> option
        tasks: Map<TaskId, Task> option
    }

    type Loaded = Map<RoomId, Room> * Map<RoomId, TaskId list> * Map<TaskId, Task>

    [<RequireQualifiedAccess>]
    type Intent =
        | None
        | Finish of Loaded

    let getIntent (state: State) =
        match state.rooms, state.roomTasks, state.tasks with
        | Some rooms, Some roomTasks, Some tasks -> (rooms, roomTasks, tasks) |> Intent.Finish
        | _ -> Intent.None

    type Msg =
        | FetchData
        | SetRooms of Room list
        | SetTasks of Task list
        | SetRoomTasks of Set<RoomTask>

    let init () : State * Cmd<Msg> =
        let state = {
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
                        Cmd.OfAsync.perform housekeepingApi.getRooms () SetRooms
                        Cmd.OfAsync.perform housekeepingApi.getTasks () SetTasks
                        Cmd.OfAsync.perform housekeepingApi.getRoomTasks () SetRoomTasks
                    ]
                    |> Cmd.batch

                state, cmd
            | SetRooms rooms ->
                let rooms = rooms |> List.map (fun room -> room.id, room) |> Map.ofList
                { state with rooms = Some rooms }, Cmd.none
            | SetTasks tasks ->
                let tasks = tasks |> List.map (fun task -> task.id, task) |> Map.ofList
                { state with tasks = Some tasks }, Cmd.none
            | SetRoomTasks roomTasks ->
                let roomTasks =
                    roomTasks
                    |> Set.toList
                    |> List.groupBy (fun roomTask -> roomTask.room)
                    |> Map.ofList
                    |> Map.map (fun _ taskList -> List.map (fun (roomTask: RoomTask) -> roomTask.task) taskList)

                let state =
                    { state with
                        roomTasks = Some roomTasks
                    }

                state, Cmd.none

        state, cmd, getIntent state

    open Feliz
    open Feliz.Bulma

    let render (_: State) (_: Dispatch<Msg>) = Bulma.block [ prop.text "Loading..." ]
