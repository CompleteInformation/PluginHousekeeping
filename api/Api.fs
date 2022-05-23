namespace CompleteInformation.Plugins.Housekeeping.Api

type Room = { name: string }

type HousekeepingApi = { getRooms: unit -> Async<Room list> }
