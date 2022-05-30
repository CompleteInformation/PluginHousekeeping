namespace CompleteInformation.Plugins.Housekeeping.Backend.Web

open CompleteInformation.Core
open CompleteInformation.Base.Backend.Web
open CompleteInformation.Plugins.Housekeeping.Api
open FSharp.Core

// For now we work with stub data to design the GUI
[<AutoOpen>]
module Data =
    let rooms =
        [
            "Kitchen"
            "Livingroom"
            "Bedroom"
            "Bathroom"
        ]
        |> List.map Room.create

    let tasks =
        [ "Vacuum"; "CleanToilette" ]
        |> List.map Task.create

    let roomTasks =
        [
            {
                room = RoomId "kitchen"
                task = TaskId "vacuum"
            }
            {
                room = RoomId "livingroom"
                task = TaskId "vacuum"
            }
            {
                room = RoomId "bedroom"
                task = TaskId "vacuum"
            }
            {
                room = RoomId "bathroom"
                task = TaskId "vacuum"
            }
            {
                room = RoomId "bathroom"
                task = TaskId "cleantoilette"
            }
        ]

[<RequireQualifiedAccess>]
module HousekeepingApi =
    let getRooms () = async { return rooms }
    let getTasks () = async { return tasks }
    let getRoomTasks () = async { return roomTasks }

    let markTaskAsDone roomTask = async { printfn "Done: %A" roomTask }

    let instance: HousekeepingApi =
        {
            getRooms = getRooms
            getTasks = getTasks
            getRoomTasks = getRoomTasks
            markTaskAsDone = markTaskAsDone
        }

type Plugin() =
    interface WebserverPlugin with
        member _.getApi routeBuilder =
            Api.build HousekeepingApi.instance routeBuilder

        member _.getMetaData() =
            {
                id = PluginId.create "housekeeping"
                name = "Housekeeping"
            }
