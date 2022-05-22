open Fake.Core
open Fake.IO

open RunHelpers
open RunHelpers.Shortcuts
open RunHelpers.Templates

open System.IO

[<RequireQualifiedAccess>]
module Config =
    let backendProject = "backend/web/WebBackend.fsproj"
    let frontendProject = "frontend/web/WebFrontend.fsproj"
    let frontendDeployPath = "frontend/web/deploy"
    let publishPath = "./publish"

module Task =
    let restore () =
        job {
            DotNet.restoreWithTools Config.backendProject

            CreateProcess.fromRawCommand "npm" [ "install" ]
            |> CreateProcess.withWorkingDirectory "./frontend/web"
            |> Job.fromCreateProcess
        }

    let buildWebClient config =
        let cmd =
            match config with
            | Debug -> "build"
            | Release -> "build-prod"

        CreateProcess.fromRawCommand "npm" [ "run"; cmd ]
        |> CreateProcess.withWorkingDirectory "./frontend/web"
        |> Job.fromCreateProcess

    let buildWebServer config =
        DotNet.build Config.backendProject config

    let build config =
        job {
            buildWebClient config
            buildWebServer config
        }

[<EntryPoint>]
let main args =
    args
    |> List.ofArray
    |> function
        | [ "restore" ] -> Task.restore ()
        | [ "build" ] ->
            job {
                Task.restore ()
                Task.build Debug
            }
        | _ ->
            Job.error [
                "Usage: dotnet run [<command>]"
                "Look up available commands in run.fs"
            ]
    |> Job.execute
