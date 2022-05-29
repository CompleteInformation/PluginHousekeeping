namespace CompleteInformation.Plugins.Housekeeping.Frontend.Web

open Elmish
open Fable.Remoting.Client

open CompleteInformation.Base.Frontend.Web
open CompleteInformation.Plugins.Housekeeping.Api

module Index =
    type Loading =
        {
            rooms: Room list option
            roomTasks: RoomTask list option
            tasks: Task list option
        }

    type Loaded =
        {
            rooms: Room list
            roomTasks: RoomTask list
            tasks: Task list
            selectedRoom: RoomId option
        }

    type Model =
        | Loading of Loading
        | Loaded of Loaded

    module Model =
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
                    selectedRoom = None
                }
                |> Loaded
            | _ -> loading |> Loading

    type Msg =
        | FetchData
        | SetRooms of Room list
        | SetRoomTasks of RoomTask list
        | SetTasks of Task list
        | SelectRoom of RoomId
        | ClearRoom

    let housekeepingApi =
        Api.createBase ()
        |> Remoting.buildProxy<HousekeepingApi>

    let init () : Model * Cmd<Msg> =
        let model =
            Loading
                {
                    rooms = None
                    roomTasks = None
                    tasks = None
                }

        let cmd = Cmd.ofMsg FetchData

        model, cmd

    let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
        match msg with
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
                    { model with rooms = Some rooms }
                    |> Model.checkIfLoaded
                | _ -> failwith "SetRooms: model is not Loading"

            model, Cmd.none
        | SetTasks tasks ->
            let model =
                match model with
                | Loading model ->
                    { model with tasks = Some tasks }
                    |> Model.checkIfLoaded
                | _ -> failwith "SetTasks: model is not Loading"

            model, Cmd.none
        | SetRoomTasks roomTasks ->
            let model =
                match model with
                | Loading model ->
                    { model with
                        roomTasks = Some roomTasks
                    }
                    |> Model.checkIfLoaded
                | _ -> failwith "SetRoomTasks: model is not Loading"

            model, Cmd.none
        | SelectRoom roomId ->
            let model =
                match model with
                | Loaded model ->
                    { model with
                        selectedRoom = Some roomId
                    }
                    |> Loaded
                | _ -> failwith "SelectRoom: model is not Loaded"

            model, Cmd.none
        | ClearRoom ->
            let model =
                match model with
                | Loaded model -> { model with selectedRoom = None } |> Loaded
                | _ -> failwith "ClearRoom: model is not Loaded"

            model, Cmd.none

    open Feliz
    open Feliz.Bulma

    let roomSelect rooms dispatch =
        [
            Bulma.columns [
                for room: Room in rooms do
                    Bulma.column [
                        column.is3
                        prop.children [
                            Bulma.card [
                                prop.children [
                                    Bulma.cardFooter [
                                        Bulma.cardFooterItem.div [
                                            Html.button [
                                                prop.className "button"
                                                prop.text room.name
                                                prop.onClick (fun _ -> SelectRoom room.id |> dispatch)
                                            ]
                                        ]
                                    ]
                                ]
                            ]
                        ]
                    ]
            ]
        ]

    let taskSelect room = []

    let loadedView model dispatch =
        match model.selectedRoom with
        | Some room -> taskSelect room
        | None -> roomSelect model.rooms dispatch


    let view (model: Model) (dispatch: Msg -> unit) =
        Html.div [
            Bulma.container [
                Bulma.title "Housekeeping"
                yield!
                    match model with
                    | Loaded model -> loadedView model dispatch
                    | Loading _ ->
                        [
                            Bulma.block [ prop.text "Loading..." ]
                        ]
            ]
        ]
