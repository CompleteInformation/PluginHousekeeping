namespace CompleteInformation.Plugins.Housekeeping.Frontend.Web

open Elmish
open Fable.Remoting.Client

open CompleteInformation.Base.Frontend.Web
open CompleteInformation.Plugins.Housekeeping.Api

module Index =
    type View =
        | Overview
        | Room of RoomId
        | NewRoom of NewRoom.State
        | NewTask of NewTask.State

    // TODO: move into own file
    type Loading = {
        rooms: Map<RoomId, Room> option
        roomTasks: Map<RoomId, TaskId list> option
        tasks: Map<TaskId, Task> option
    }

    type Loaded = {
        // Global data
        rooms: Map<RoomId, Room>
        roomTasks: Map<RoomId, TaskId list>
        tasks: Map<TaskId, Task>
        // View-specific data
        view: View
    }

    type State =
        | Loading of Loading
        | Loaded of Loaded

    module State =
        let checkIfLoaded (loading: Loading) =
            match loading with
            | {
                  rooms = Some rooms
                  roomTasks = Some roomTasks
                  tasks = Some tasks
              } ->
                {
                    rooms = rooms
                    roomTasks = roomTasks
                    tasks = tasks
                    view = Overview
                }
                |> Loaded
            | _ -> loading |> Loading

    type Msg =
        | SetView of View
        | FetchData
        | SetRooms of Room list
        | SetTasks of Task list
        | SetRoomTasks of Set<RoomTask>
        | SelectRoom of RoomId
        // Child messages
        | NewRoomMsg of NewRoom.Msg
        | NewTaskMsg of NewTask.Msg

    let setNewRoomView = NewRoom.init () |> View.NewRoom |> SetView
    let setNewTaskView = NewTask.init () |> View.NewTask |> SetView

    let housekeepingApi = Api.createBase () |> Remoting.buildProxy<HousekeepingApi>

    let init () : State * Cmd<Msg> =
        let model =
            Loading
                {
                    rooms = None
                    roomTasks = None
                    tasks = None
                }

        let cmd = Cmd.ofMsg FetchData

        model, cmd

    let update (msg: Msg) (model: State) : State * Cmd<Msg> =
        match msg with
        | SetView view ->
            match model with
            | Loaded model -> Loaded { model with view = view }, Cmd.none
            | _ -> model, Cmd.none
        | FetchData ->
            let cmd =
                [
                    Cmd.OfAsync.perform housekeepingApi.getRooms () SetRooms
                    Cmd.OfAsync.perform housekeepingApi.getTasks () SetTasks
                    Cmd.OfAsync.perform housekeepingApi.getRoomTasks () SetRoomTasks
                ]
                |> Cmd.batch

            model, cmd
        | SetRooms rooms ->
            let model =
                match model with
                | Loading model ->
                    let rooms = rooms |> List.map (fun room -> room.id, room) |> Map.ofList

                    { model with rooms = Some rooms } |> State.checkIfLoaded
                | _ -> failwith "SetTasks: model is not Loading"

            model, Cmd.none
        | SetTasks tasks ->
            let model =
                match model with
                | Loading model ->
                    let tasks = tasks |> List.map (fun task -> task.id, task) |> Map.ofList

                    { model with tasks = Some tasks } |> State.checkIfLoaded
                | _ -> failwith "SetTasks: model is not Loading"

            model, Cmd.none
        | SetRoomTasks roomTasks ->
            let model =
                match model with
                | Loading model ->
                    let roomTasks =
                        roomTasks
                        |> Set.toList
                        |> List.groupBy (fun roomTask -> roomTask.room)
                        |> Map.ofList
                        |> Map.map (fun _ taskList -> List.map (fun (roomTask: RoomTask) -> roomTask.task) taskList)

                    { model with
                        roomTasks = Some roomTasks
                    }
                    |> State.checkIfLoaded
                | _ -> failwith "SetRoomTasks: model is not Loading"

            model, Cmd.none
        | SelectRoom roomId ->
            let model =
                match model with
                | Loaded model -> { model with view = Room roomId } |> Loaded
                | _ -> failwith "SelectRoom: model is not Loaded"

            model, Cmd.none
        // Child updates
        | NewRoomMsg msg ->
            match model with
            | Loaded ({ view = NewRoom childState } as state) ->
                let childState, childCmd, intent = NewRoom.update housekeepingApi msg childState

                let state =
                    match intent with
                    | NewRoom.Intent.None -> { state with view = NewRoom childState }
                    | NewRoom.Intent.Cancel -> { state with view = Overview }
                    | NewRoom.Intent.Finish room ->
                        { state with
                            rooms = Map.add room.id room state.rooms
                            view = Overview
                        }

                Loaded state, Cmd.map NewRoomMsg childCmd
            | _ -> model, Cmd.none
        | NewTaskMsg msg ->
            match model with
            | Loaded ({ view = NewTask childState } as state) ->
                let childState, childCmd, intent = NewTask.update housekeepingApi msg childState

                let state =
                    match intent with
                    | NewTask.Intent.None -> { state with view = NewTask childState }
                    | NewTask.Intent.Cancel -> { state with view = Overview }
                    | NewTask.Intent.Finish task ->
                        { state with
                            tasks = Map.add task.id task state.tasks
                            view = Overview
                        }

                Loaded state, Cmd.map NewTaskMsg childCmd
            | _ -> model, Cmd.none

    open Feliz
    open Feliz.Bulma

    let columnView items itemView =
        Bulma.columns [
            prop.className "is-multiline"
            prop.children [
                for item in items do
                    Bulma.column [ column.is3; prop.children (itemView item: ReactElement) ]
            ]
        ]

    let roomSelect dispatch rooms =
        Html.div [
            Html.div [
                prop.className "buttons"
                prop.children [
                    Bulma.button.button[prop.text "New Room"
                                        prop.onClick (fun _ -> setNewRoomView |> dispatch)]
                    Bulma.button.button[prop.text "New Task"
                                        prop.onClick (fun _ -> setNewTaskView |> dispatch)]
                ]
            ]
            columnView rooms (fun (room: Room) ->
                Bulma.card [
                    prop.children [
                        Bulma.cardFooter [
                            Bulma.cardFooterItem.div [
                                Bulma.button.button [
                                    prop.text room.properties.name
                                    prop.onClick (fun _ -> SelectRoom room.id |> dispatch)
                                ]
                            ]
                        ]
                    ]
                ])
        ]

    let taskSelect dispatch tasks =
        columnView tasks (fun (task: Task) ->
            Bulma.card [
                prop.children [
                    Bulma.cardFooter [
                        Bulma.cardFooterItem.div [
                            Bulma.button.button [
                                prop.text task.properties.name
                                prop.onClick (fun _ -> SetView Overview |> dispatch)
                            ]
                        ]
                    ]
                ]
            ])

    let view (state: State) (dispatch: Msg -> unit) =
        Html.div [
            Bulma.container [
                Bulma.title "Housekeeping"
                match state with
                | Loading _ -> Bulma.block [ prop.text "Loading..." ]
                | Loaded ({ view = Overview } as state) ->
                    roomSelect dispatch (state.rooms |> Map.toList |> List.map snd)
                | Loaded ({ view = (Room roomId) } as state) ->
                    Map.find roomId state.roomTasks
                    |> List.map (fun taskId -> Map.find taskId state.tasks)
                    |> taskSelect dispatch
                // Child views
                | Loaded { view = NewRoom state } -> NewRoom.render state (NewRoomMsg >> dispatch)
                | Loaded { view = NewTask state } -> NewTask.render state (NewTaskMsg >> dispatch)
            ]
        ]
