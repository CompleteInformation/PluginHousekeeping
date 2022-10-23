namespace CompleteInformation.Plugins.Housekeeping.Frontend.Web

open Elmish
open Fable.Remoting.Client

open CompleteInformation.Base.Frontend.Web

open CompleteInformation.Plugins.Housekeeping.Api

type Msg =
    | CreateNewRoom of RoomProperties
    | CreateNewTask of TaskProperties
    | AddRoom of Room
    | AddTask of Task
    | SetView of View
    // Child messages
    | LoadingMsg of Loading.Msg
    | ManagerMsg of Manager.Msg

module State =
    let init () : State * Cmd<Msg> =
        let state, cmd = Loading.init ()
        Loading state, Cmd.map LoadingMsg cmd

    let housekeepingApi = Api.createBase () |> Remoting.buildProxy<HousekeepingApi>

    let update (msg: Msg) (model: State) : State * Cmd<Msg> =
        match msg with
        | SetView view ->
            match model with
            | Loaded model -> Loaded { model with view = view }, Cmd.none
            | _ -> model, Cmd.none
        // Child updates
        | LoadingMsg msg ->
            match model with
            | Loading childState ->
                let childState, childCmd, intent = Loading.update housekeepingApi msg childState

                let state =
                    match intent with
                    | Loading.Intent.None -> Loading childState
                    | Loading.Intent.Finish (rooms, roomTasks, tasks) ->
                        {
                            globalData =
                                {
                                    rooms = rooms
                                    roomTasks = roomTasks
                                    tasks = tasks
                                }
                            view = View.Overview
                        }
                        |> Loaded

                state, Cmd.map LoadingMsg childCmd
            | _ -> model, Cmd.none
        | ManagerMsg msg ->
            match model with
            | Loaded ({ view = View.Manager childState } as state) ->
                let state', cmd, intent = Manager.State.update msg state.globalData childState

                let cmd2 =
                    match intent with
                    | Manager.Intent.None -> Cmd.none
                    | Manager.Intent.Leave -> SetView View.Overview |> Cmd.ofMsg
                    | Manager.Intent.CreateNewRoom props -> CreateNewRoom props |> Cmd.ofMsg
                    | Manager.Intent.CreateNewTask props -> CreateNewTask props |> Cmd.ofMsg

                let cmd = Cmd.batch [ Cmd.map ManagerMsg cmd; cmd2 ]

                Loaded
                    { state with
                        view = View.Manager state'
                    },
                cmd
            | _ -> model, Cmd.none
        | CreateNewRoom properties ->
            let cmd = Cmd.OfAsync.perform housekeepingApi.putRoom properties AddRoom
            model, cmd
        | AddRoom room ->
            match model with
            | Loaded state ->
                // TODO: Lens
                let state =
                    { state with
                        globalData =
                            { state.globalData with
                                rooms = Map.add room.id room state.globalData.rooms
                            }
                    }

                Loaded state, Cmd.none
            | _ -> model, Cmd.none
        | CreateNewTask properties ->
            let cmd = Cmd.OfAsync.perform housekeepingApi.putTask properties AddTask
            model, cmd
        | AddTask task ->
            match model with
            | Loaded state ->
                // TODO: Lens
                let state =
                    { state with
                        globalData =
                            { state.globalData with
                                tasks = Map.add task.id task state.globalData.tasks
                            }
                    }

                Loaded state, Cmd.none
            | _ -> model, Cmd.none
