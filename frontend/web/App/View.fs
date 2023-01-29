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

    let roomSelect dispatch rooms = [
        Bulma.buttons [
            Bulma.button.button[prop.text "Manage"
                                prop.onClick (fun _ -> setManagerView |> dispatch)]
        ]
        columnView rooms (fun (room: Room) ->
            Bulma.card [
                prop.children [
                    Bulma.cardFooter [
                        Bulma.cardFooterItem.div [
                            Bulma.button.button [
                                prop.text room.properties.name
                                prop.onClick (fun _ -> View.Room(room.id, [], []) |> SetView |> dispatch)
                            ]
                        ]
                    ]
                ]
            ])
    ]

    let taskSelect roomId loadingTasks loadedTasks dispatch tasks = [
        Bulma.buttons [
            Bulma.button.button[prop.text "Back"
                                prop.onClick (fun _ -> SetView View.Overview |> dispatch)]
        ]
        columnView tasks (fun (task: Task) ->
            Bulma.card [
                prop.children [
                    Bulma.cardFooter [
                        Bulma.cardFooterItem.div [
                            Bulma.button.button [
                                if List.contains task.id loadingTasks then
                                    Bulma.button.isLoading
                                    prop.disabled true

                                if List.contains task.id loadedTasks then
                                    Bulma.color.isSuccess
                                    prop.disabled true

                                prop.children [
                                    if List.contains task.id loadedTasks then
                                        Bulma.icon [ Html.i [ prop.className "fas fa-check" ] ]
                                    Html.span [ prop.text task.properties.name ]
                                ]
                                prop.onClick (fun _ -> Track { room = roomId; task = task.id } |> dispatch)
                            ]
                        ]
                    ]
                ]
            ])
    ]

    let render (state: State) (dispatch: Msg -> unit) =
        Bulma.container [
            Bulma.title [ title.is1; prop.text "Housekeeping" ]
            match state with
            | Loaded({ view = View.Overview } as state) ->
                yield! roomSelect dispatch (state.globalData.rooms |> Map.toList |> List.map snd)
            | Loaded({
                         view = View.Room(roomId, loadingTasks, loadedTasks)
                     } as state) ->
                yield!
                    Map.find roomId state.globalData.roomTasks.perRoom
                    |> List.map (fun taskId -> Map.find taskId state.globalData.tasks)
                    |> taskSelect roomId loadingTasks loadedTasks dispatch
            // Child views
            | Loading state -> Loading.render state (LoadingMsg >> dispatch)
            | Loaded({ view = View.Manager childState } as state) ->
                yield! Manager.View.render state.globalData childState (ManagerMsg >> dispatch)
        ]
