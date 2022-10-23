namespace CompleteInformation.Plugins.Housekeeping.Frontend.Web.Manager

open Elmish
open Fable.Remoting.Client

open CompleteInformation.Base.Frontend.Web

open CompleteInformation.Plugins.Housekeeping.Api
open CompleteInformation.Plugins.Housekeeping.Frontend.Web

[<RequireQualifiedAccess>]
type Intent =
    | None
    | Leave
    | CreateNewRoom of RoomProperties
    | CreateNewTask of TaskProperties
    | AddRoomTask of RoomTask
    | RemoveRoomTask of RoomTask

type Msg =
    | SetNewRoomName of string
    | SetNewTaskName of string
    | CreateNewRoom
    | CreateNewTask
    | AddRoomTask of RoomTask
    | RemoveRoomTask of RoomTask
    | Leave

module State =
    let init () : State = { newRoomName = ""; newTaskName = "" }

    let update (msg: Msg) (globalState: GlobalState) (state: State) : State * Cmd<Msg> * Intent =
        match msg with
        | Leave -> state, Cmd.none, Intent.Leave
        // TODO: lens
        | SetNewRoomName name ->
            let state = { state with newRoomName = name }

            state, Cmd.none, Intent.None
        | SetNewTaskName name ->
            let state = { state with newTaskName = name }

            state, Cmd.none, Intent.None
        | CreateNewRoom ->
            let props: RoomProperties = { name = state.newRoomName }
            { state with newRoomName = "" }, Cmd.none, Intent.CreateNewRoom props
        | CreateNewTask ->
            let props: TaskProperties = { name = state.newTaskName }
            { state with newTaskName = "" }, Cmd.none, Intent.CreateNewTask props
        | AddRoomTask roomTask -> state, Cmd.none, Intent.AddRoomTask roomTask
        | RemoveRoomTask roomTask -> state, Cmd.none, Intent.RemoveRoomTask roomTask
