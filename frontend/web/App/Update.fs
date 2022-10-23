namespace CompleteInformation.Plugins.Housekeeping.Frontend.Web

open Elmish
open Fable.Remoting.Client

open CompleteInformation.Base.Frontend.Web

open CompleteInformation.Plugins.Housekeeping.Api

type Msg =
    | SetView of View
    // Child messages
    | LoadingMsg of Loading.Msg
    | NewRoomMsg of NewRoom.Msg
    | NewTaskMsg of NewTask.Msg

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
                            rooms = rooms
                            roomTasks = roomTasks
                            tasks = tasks
                            view = View.Overview
                        }
                        |> Loaded

                state, Cmd.map LoadingMsg childCmd
            | _ -> model, Cmd.none
        | NewRoomMsg msg ->
            match model with
            | Loaded ({ view = View.NewRoom childState } as state) ->
                let childState, childCmd, intent = NewRoom.update housekeepingApi msg childState

                let state =
                    match intent with
                    | NewRoom.Intent.None ->
                        { state with
                            view = View.NewRoom childState
                        }
                    | NewRoom.Intent.Cancel -> { state with view = View.Overview }
                    | NewRoom.Intent.Finish room ->
                        { state with
                            rooms = Map.add room.id room state.rooms
                            view = View.Overview
                        }

                Loaded state, Cmd.map NewRoomMsg childCmd
            | _ -> model, Cmd.none
        | NewTaskMsg msg ->
            match model with
            | Loaded ({ view = View.NewTask childState } as state) ->
                let childState, childCmd, intent = NewTask.update housekeepingApi msg childState

                let state =
                    match intent with
                    | NewTask.Intent.None ->
                        { state with
                            view = View.NewTask childState
                        }
                    | NewTask.Intent.Cancel -> { state with view = View.Overview }
                    | NewTask.Intent.Finish task ->
                        { state with
                            tasks = Map.add task.id task state.tasks
                            view = View.Overview
                        }

                Loaded state, Cmd.map NewTaskMsg childCmd
            | _ -> model, Cmd.none
