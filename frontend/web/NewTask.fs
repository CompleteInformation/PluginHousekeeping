namespace CompleteInformation.Plugins.Housekeeping.Frontend.Web

open Elmish

open CompleteInformation.Plugins.Housekeeping.Api

module NewTask =
    type State = { name: string }

    type Intent =
        | None
        | Finish of Task
        | Cancel

    type Msg =
        | SetName of string
        | Submit
        | Finish of Task
        | Cancel

    let init () : State = { name = "" }

    let update housekeepingApi msg (state: State) =
        match msg with
        | SetName name -> { state with name = name }, Cmd.none, None
        | Submit ->
            let taskProperties = { TaskProperties.name = state.name }
            state, Cmd.OfAsync.perform housekeepingApi.putTask taskProperties Finish, None
        | Finish task -> state, Cmd.none, Intent.Finish task
        | Cancel -> state, Cmd.none, Intent.Cancel

    open Feliz
    open Feliz.Bulma

    let render state dispatch =
        Html.div [
            Bulma.field.div [
                Bulma.label "Name"
                Bulma.control.div [
                    Bulma.input.text [
                        prop.placeholder "Name of the new task"
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
