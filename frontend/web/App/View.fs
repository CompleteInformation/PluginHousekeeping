namespace CompleteInformation.Plugins.Housekeeping.Frontend.Web

open CompleteInformation.Plugins.Housekeeping.Api

module View =
    let setNewRoomView = NewRoom.init () |> View.NewRoom |> SetView
    let setNewTaskView = NewTask.init () |> View.NewTask |> SetView

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
                                    prop.onClick (fun _ -> View.Room room.id |> SetView |> dispatch)
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
                                prop.onClick (fun _ -> SetView View.Overview |> dispatch)
                            ]
                        ]
                    ]
                ]
            ])

    let render (state: State) (dispatch: Msg -> unit) =
        Html.div [
            Bulma.container [
                Bulma.title "Housekeeping"
                match state with
                | Loading state -> Loading.render state (LoadingMsg >> dispatch)
                | Loaded ({ view = View.Overview } as state) ->
                    roomSelect dispatch (state.rooms |> Map.toList |> List.map snd)
                | Loaded ({ view = View.Room roomId } as state) ->
                    Map.find roomId state.roomTasks
                    |> List.map (fun taskId -> Map.find taskId state.tasks)
                    |> taskSelect dispatch
                // Child views
                | Loaded { view = View.NewRoom state } -> NewRoom.render state (NewRoomMsg >> dispatch)
                | Loaded { view = View.NewTask state } -> NewTask.render state (NewTaskMsg >> dispatch)
            ]
        ]
