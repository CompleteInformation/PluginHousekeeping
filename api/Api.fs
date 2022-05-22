namespace CompleteInformation.Plugins.Housekeeping.Api

type Room = { name: string }

type Api = { getRooms: unit -> Async<Room list> }
