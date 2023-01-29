namespace CompleteInformation.Plugins.Housekeeping.Frontend.Web

open Elmish
open Fable.Remoting.Client
open SimpleOptics
open System

open CompleteInformation.Base.Frontend.Web

open CompleteInformation.Plugins.Housekeeping.Api

type Msg =
    | AddRoom of Room
    | AddTask of Task
    | AddRoomTask of RoomTask
    | RemoveRoomTask of RoomTask
    | SetView of View
    | Track of RoomTask
    | TrackFinished of RoomTask
    // Child messages
    | LoadingMsg of Loading.Msg
    | ManagerMsg of Manager.Msg

module State =
    let init () : State * Cmd<Msg> =
        let state, cmd = Loading.init ()
        Loading state, Cmd.map LoadingMsg cmd

    let housekeepingApi = Api.createBase () |> Remoting.buildProxy<HousekeepingApi>

    let createNewRoomCmd properties =
        Cmd.OfAsync.perform housekeepingApi.putRoom properties AddRoom

    let createNewTaskCmd properties =
        Cmd.OfAsync.perform housekeepingApi.putTask properties AddTask

    let createRoomTaskCmd roomTask =
        Cmd.OfAsync.perform housekeepingApi.putRoomTask roomTask (fun () -> AddRoomTask roomTask)

    let deleteRoomTaskCmd roomTask =
        Cmd.OfAsync.perform housekeepingApi.deleteRoomTask roomTask (fun () -> RemoveRoomTask roomTask)

    let trackRoomTaskCmd userId roomTask =
        Cmd.OfAsync.perform (housekeepingApi.trackRoomTaskDone userId) roomTask (fun () -> TrackFinished roomTask)

    let update (msg: Msg) (model: State) : State * Cmd<Msg> =
        match msg with
        | SetView view ->
            match model with
            | Loaded model -> Loaded { model with view = view }, Cmd.none
            | _ -> model, Cmd.none
        | Track roomTask ->
            let userId = (Optic.get StateOptic.loadedGlobalUserId model).Value

            let model =
                model
                |> Optic.map StateOptic.loadedView (fun view ->
                    match view with
                    | View.Room(roomId, loadingTasks, loadedTasks) when roomId = roomTask.room ->
                        View.Room(roomId, roomTask.task :: loadingTasks, loadedTasks)
                    | _ -> view)
                |> Optic.set
                    (StateOptic.loadedGlobalLastDoneRoomTask (roomTask.room, roomTask.task))
                    { time = DateTime.Now; user = userId }

            let cmd = trackRoomTaskCmd userId roomTask
            model, cmd
        | TrackFinished roomTask ->
            let model =
                Optic.map
                    StateOptic.loadedView
                    (fun view ->
                        match view with
                        | View.Room(roomId, loadingTasks, loadedTasks) when roomId = roomTask.room ->
                            let loadingTasks = List.filter ((<>) roomTask.task) loadingTasks
                            View.Room(roomId, loadingTasks, roomTask.task :: loadedTasks)
                        | _ -> view)
                    model

            model, Cmd.none
        | AddRoom room ->
            let state = Optic.map StateOptic.loadedGlobalRooms (Map.add room.id room) model
            state, Cmd.none
        | AddTask task ->
            let state = Optic.map StateOptic.loadedGlobalTasks (Map.add task.id task) model
            state, Cmd.none
        | AddRoomTask roomTask ->
            let state =
                model
                |> Optic.map
                    StateOptic.loadedGlobalRoomTasksPerRoom
                    (Map.change
                        roomTask.room
                        (function
                         | None -> [ roomTask.task ]
                         | Some lst -> roomTask.task :: lst
                         >> Some))
                |> Optic.map
                    StateOptic.loadedGlobalRoomTasksPerTask
                    (Map.change
                        roomTask.task
                        (function
                         | None -> [ roomTask.room ]
                         | Some lst -> roomTask.room :: lst
                         >> Some))

            state, Cmd.none
        | RemoveRoomTask roomTask ->
            let state =
                model
                |> Optic.map
                    StateOptic.loadedGlobalRoomTasksPerRoom
                    (Map.change
                        roomTask.room
                        (function
                         | None -> []
                         | Some lst -> List.filter ((<>) roomTask.task) lst
                         >> Some))
                |> Optic.map
                    StateOptic.loadedGlobalRoomTasksPerTask
                    (Map.change
                        roomTask.task
                        (function
                         | None -> []
                         | Some lst -> List.filter ((<>) roomTask.room) lst
                         >> Some))

            state, Cmd.none
        // Child updates
        | LoadingMsg msg ->
            match model with
            | Loading childState ->
                let childState, childCmd, intent = Loading.update housekeepingApi msg childState

                let state =
                    match intent with
                    | Loading.Intent.None -> Loading childState
                    | Loading.Intent.Finish(lastDone, rooms, roomTasks, tasks) ->
                        {
                            globalData =
                                {
                                    userId = (LocalStorage.getUserId ()).Value
                                    lastDone =
                                        lastDone
                                        |> Map.toSeq
                                        |> Seq.map (fun (rt, metadata) -> (rt.room, rt.task), metadata)
                                        |> Map.ofSeq
                                    rooms = rooms
                                    roomTasks =
                                        {
                                            // TODO: Abstract away
                                            perRoom =
                                                roomTasks
                                                |> Set.toList
                                                |> List.map (fun rt -> rt.room, rt.task)
                                                |> List.groupBy fst
                                                |> List.map (fun (roomId, lst) -> roomId, List.map snd lst)
                                                |> Map.ofList
                                            perTask =
                                                roomTasks
                                                |> Set.toList
                                                |> List.map (fun rt -> rt.task, rt.room)
                                                |> List.groupBy fst
                                                |> List.map (fun (taskId, lst) -> taskId, List.map snd lst)
                                                |> Map.ofList
                                        }
                                    tasks = tasks
                                }
                            view = View.Overview
                        }
                        |> Loaded

                state, Cmd.map LoadingMsg childCmd
            | _ -> model, Cmd.none
        | ManagerMsg msg ->
            match model with
            | Loaded({ view = View.Manager childState } as state) ->
                let state', cmd, intent = Manager.State.update msg state.globalData childState

                let cmd2 =
                    match intent with
                    | Manager.Intent.None -> Cmd.none
                    | Manager.Intent.Leave -> SetView View.Overview |> Cmd.ofMsg
                    | Manager.Intent.CreateNewRoom props -> createNewRoomCmd props
                    | Manager.Intent.CreateNewTask props -> createNewTaskCmd props
                    | Manager.Intent.AddRoomTask roomTask -> createRoomTaskCmd roomTask
                    | Manager.Intent.RemoveRoomTask roomTask -> deleteRoomTaskCmd roomTask

                let cmd = Cmd.batch [ Cmd.map ManagerMsg cmd; cmd2 ]

                Loaded
                    { state with
                        view = View.Manager state'
                    },
                cmd
            | _ -> model, Cmd.none
