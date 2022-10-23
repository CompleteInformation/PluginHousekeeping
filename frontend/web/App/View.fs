namespace CompleteInformation.Plugins.Housekeeping.Frontend.Web

open CompleteInformation.Plugins.Housekeeping.Api

module View =
    let setManagerView = Manager.State.init () |> View.Manager |> SetView

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
                    Bulma.button.button[prop.text "Manage"
                                        prop.onClick (fun _ -> setManagerView |> dispatch)]
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
                Bulma.title [ title.is1; prop.text "Housekeeping" ]
                match state with
                | Loaded ({ view = View.Overview } as state) ->
                    roomSelect dispatch (state.globalData.rooms |> Map.toList |> List.map snd)
                | Loaded ({ view = View.Room roomId } as state) ->
                    Map.find roomId state.globalData.roomTasks
                    |> List.map (fun taskId -> Map.find taskId state.globalData.tasks)
                    |> taskSelect dispatch
                // Child views
                | Loading state -> Loading.render state (LoadingMsg >> dispatch)
                | Loaded ({ view = View.Manager childState } as state) ->
                    yield! Manager.View.render state.globalData childState (ManagerMsg >> dispatch)
            //| Loaded { view = View.NewRoom state } -> NewRoom.render state (NewRoomMsg >> dispatch)
            //| Loaded { view = View.NewTask state } -> NewTask.render state (NewTaskMsg >> dispatch)
            ]
        ]
