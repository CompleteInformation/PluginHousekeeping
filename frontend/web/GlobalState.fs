namespace CompleteInformation.Plugins.Housekeeping.Frontend.Web

open CompleteInformation.Plugins.Housekeeping.Api

type GlobalState = {
    rooms: Map<RoomId, Room>
    roomTasks: Map<RoomId, TaskId list>
    tasks: Map<TaskId, Task>
}
