namespace CompleteInformation.Plugins.Housekeeping.Frontend.Web

open System

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

    let taskSelect roomId tasksWithLastDone loadingTasks loadedTasks dispatch = [
        Bulma.buttons [
            Bulma.button.button[prop.text "Back"
                                prop.onClick (fun _ -> SetView View.Overview |> dispatch)]
        ]
        columnView tasksWithLastDone (fun (task: Task, lastDone: HistoryMetadata option) ->
            Bulma.card [
                prop.children [
                    match lastDone with
                    | Some lastDone ->
                        let yesterday = DateTime.Now.AddDays(-1)
                        let lastWeek = DateTime.Now.AddDays(-7)

                        let timeString =
                            if lastDone.time > yesterday then
                                lastDone.time.ToString "HH:mm"
                            else if lastDone.time.Date = yesterday.Date then
                                "Yesterday"
                            else if lastDone.time.Date > lastWeek.Date then
                                "Last " + lastDone.time.DayOfWeek.ToString()
                            else
                                lastDone.time.ToString "yyyy/MM/dd"

                        Bulma.cardImage [
                            Bulma.notification [ Bulma.color.isInfo; prop.text $"Last done: %s{timeString}" ]
                        ]
                    | None -> ()
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
                let tasksWithLastDone =
                    Map.find roomId state.globalData.roomTasks.perRoom
                    |> List.map (fun taskId ->
                        let task = Map.find taskId state.globalData.tasks
                        let lastDone = Map.tryFind (roomId, taskId) state.globalData.lastDone
                        task, lastDone)

                yield! taskSelect roomId tasksWithLastDone loadingTasks loadedTasks dispatch
            // Child views
            | Loading state -> Loading.render state (LoadingMsg >> dispatch)
            | Loaded({ view = View.Manager childState } as state) ->
                yield! Manager.View.render state.globalData childState (ManagerMsg >> dispatch)
        ]
