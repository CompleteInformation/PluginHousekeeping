namespace CompleteInformation.Plugins.Housekeeping.Frontend.Web

open Elmish
open Fable.Remoting.Client

open CompleteInformation.Plugins.Housekeeping.Api

module Index =
    type Model =
        {
            /// List of rooms available
            rooms: Room list option
        }

    type Msg =
        | FetchRooms
        | SetRooms of Room list

    let housekeepingApi =
        Remoting.createApi ()
        |> Remoting.withBaseUrl "http://localhost:8084/api"
        |> Remoting.buildProxy<HousekeepingApi>

    let init () : Model * Cmd<Msg> =
        let model = { rooms = None }

        let cmd = Cmd.ofMsg FetchRooms

        model, cmd

    let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
        match msg with
        | FetchRooms ->
            let cmd = Cmd.OfAsync.perform housekeepingApi.getRooms () SetRooms

            model, cmd
        | SetRooms rooms -> { model with rooms = Some rooms }, Cmd.none

    open Feliz
    open Feliz.Bulma

    let content model =
        [
            Bulma.title "Housekeeping"
            let text =
                match model.rooms with
                | Some rooms ->
                    rooms
                    |> List.map (fun room -> room.name)
                    |> String.concat ","
                | _ -> "Loading..."

            Bulma.block [ prop.text text ]
        ]

    let view (model: Model) (dispatch: Msg -> unit) =
        Html.div [
            Bulma.container (content model)
        ]
