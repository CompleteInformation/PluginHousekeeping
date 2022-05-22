namespace CompleteInformation.Plugins.Housekeeping.Backend.Web

open CompleteInformation.PluginBase
open CompleteInformation.Plugins.Housekeeping.Api
open FSharp.Core

[<RequireQualifiedAccess>]
module HousekeepingApi =
    let getRooms () =
        async {
            return
                [
                    "kitchen"
                    "livingroom"
                    "bedroom"
                    "bathroom"
                ]
                |> List.map (fun name -> { name = name })
        }

    let instance: Api = { getRooms = getRooms }

type HouseKeepingPlugin() =
    interface WebserverPlugin with
        member _.getApi routeBuilder =
            Api.build HousekeepingApi.instance routeBuilder

        member _.getMetaData() =
            {
                id = PluginId.create "housekeeping"
                name = "Housekeeping"
            }
