namespace CompleteInformation.Plugins.Housekeeping.Frontend.Web.Manager

open CompleteInformation.Plugins.Housekeeping.Api
open CompleteInformation.Plugins.Housekeeping.Frontend.Web

module View =
    open Feliz
    open Feliz.Bulma

    // TODO: Move to viewhelper
    [<RequireQualifiedAccess>]
    module Bulma =
        let inline multilineColumns (children: ReactElement seq) =
            Bulma.columns [ columns.isMultiline; prop.children children ]

    let inline itemCard (cardProps: ReactElement seq) =
        Bulma.column [ column.is6; prop.children [ Bulma.card cardProps ] ]

    let inline itemCardFullWidth (cardProps: ReactElement seq) =
        Bulma.column [ column.is12; prop.children [ Bulma.card cardProps ] ]

    let renderCreateNew placeholder (value: string) changeMsg createMsg dispatch =
        itemCardFullWidth [
            Bulma.cardContent [
                Bulma.field.div [
                    Bulma.label "Name"
                    Bulma.control.div [
                        Bulma.input.text [
                            prop.placeholder placeholder
                            prop.value value
                            prop.onChange (fun (name: string) -> changeMsg name |> dispatch)
                        ]
                    ]
                ]
            ]
            Bulma.cardFooter [
                Bulma.cardFooterItem.div [
                    Bulma.button.button [ prop.text "Create"; prop.onClick (fun _ -> createMsg |> dispatch) ]
                ]
            ]
        ]

    let renderRoom (room: Room) (globalState: GlobalState) dispatch =
        let tasks =
            Map.tryFind room.id globalState.roomTasks.perRoom |> Option.defaultValue []

        itemCard [
            Bulma.cardHeader [ Bulma.cardHeaderTitle.p room.properties.name ]
            Bulma.cardContent [
                for task in globalState.tasks.Values do
                    let isChecked = List.contains task.id tasks

                    Bulma.field.div [
                        Html.label [
                            prop.className "checkbox"
                            prop.children [
                                Html.input [
                                    prop.type'.checkbox
                                    prop.isChecked isChecked
                                    prop.onCheckedChange (fun checked ->
                                        let roomTask = { room = room.id; task = task.id }

                                        if checked then
                                            AddRoomTask roomTask
                                        else
                                            RemoveRoomTask roomTask
                                        |> dispatch)
                                ]
                                Bulma.text.span $" %s{task.properties.name}"
                            ]
                        ]
                    ]
            ]
        ]

    let renderTask (task: Task) (globalState: GlobalState) dispatch =
        let rooms =
            Map.tryFind task.id globalState.roomTasks.perTask |> Option.defaultValue []

        itemCard [
            Bulma.cardHeader [ Bulma.cardHeaderTitle.p task.properties.name ]
            Bulma.cardContent [
                for room in globalState.rooms.Values do
                    let isChecked = List.contains room.id rooms

                    Bulma.field.div [
                        Html.label [
                            prop.className "checkbox"
                            prop.children [
                                Html.input [
                                    prop.type'.checkbox
                                    prop.isChecked isChecked
                                    prop.onCheckedChange (fun checked ->
                                        let roomTask = { room = room.id; task = task.id }

                                        if checked then
                                            AddRoomTask roomTask
                                        else
                                            RemoveRoomTask roomTask
                                        |> dispatch)
                                ]
                                Bulma.text.span $" %s{room.properties.name}"
                            ]
                        ]
                    ]
            ]
        ]

    let render globalState state (dispatch: Msg -> unit) : ReactElement list = [
        Bulma.buttons [
            Bulma.button.button [ prop.text "Back"; prop.onClick (fun _ -> Leave |> dispatch) ]
        ]
        Bulma.columns [
            Bulma.column [
                Bulma.title [ title.is4; prop.text "Rooms" ]
                Bulma.multilineColumns [
                    renderCreateNew "e.g. Kitchen" state.newRoomName SetNewRoomName CreateNewRoom dispatch
                    for room in globalState.rooms.Values do
                        renderRoom room globalState dispatch
                ]
            ]
            Bulma.column [
                Bulma.title [ title.is4; prop.text "Tasks" ]
                Bulma.multilineColumns [
                    renderCreateNew "e.g. Clean" state.newTaskName SetNewTaskName CreateNewTask dispatch
                    for task in globalState.tasks.Values do
                        renderTask task globalState dispatch
                ]
            ]
        ]
    ]
