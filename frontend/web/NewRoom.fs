namespace CompleteInformation.Plugins.Housekeeping.Frontend.Web

open Elmish

open CompleteInformation.Plugins.Housekeeping.Api

module NewRoom =
    type State = { name: string }

    [<RequireQualifiedAccess>]
    type Intent =
        | None
        | Finish of Room
        | Cancel

    type Msg =
        | SetName of string
        | Submit
        | Finish of Room
        | Cancel

    let init () : State = { name = "" }

    let update housekeepingApi msg (state: State) =
        match msg with
        | SetName name -> { state with name = name }, Cmd.none, Intent.None
        | Submit ->
            let roomProperties = { RoomProperties.name = state.name }
            state, Cmd.OfAsync.perform housekeepingApi.putRoom roomProperties Finish, Intent.None
        | Finish room -> state, Cmd.none, Intent.Finish room
        | Cancel -> state, Cmd.none, Intent.Cancel

    open Feliz
    open Feliz.Bulma

    let render state dispatch =
        Html.div [
            Bulma.field.div [
                Bulma.label "Name"
                Bulma.control.div [
                    Bulma.input.text [
                        prop.placeholder "Name of the new room"
                        prop.value state.name
                        prop.onChange (SetName >> dispatch)
                    ]
                ]
            ]
            Html.div [
                prop.className "buttons"
                prop.children [
                    Bulma.button.button [
                        Bulma.color.isLight
                        prop.text "Cancel"
                        prop.onClick (fun _ -> Cancel |> dispatch)
                    ]
                    Bulma.button.button [
                        Bulma.color.isPrimary
                        prop.text "Submit"
                        prop.onClick (fun _ -> Submit |> dispatch)
                    ]
                ]
            ]
        ]
