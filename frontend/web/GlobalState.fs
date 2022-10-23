namespace CompleteInformation.Plugins.Housekeeping.Frontend.Web

open CompleteInformation.Plugins.Housekeeping.Api

type GlobalState = {
    rooms: Map<RoomId, Room>
    tasks: Map<TaskId, Task>
    roomTasks: {| perRoom: Map<RoomId, TaskId list>
                  perTask: Map<TaskId, RoomId list> |}
}
