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
    let serverArchivePath = "./CompleteInformation.tar.lz"
    let serverPath = "./server"

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

    let publish config =
        let config =
            match config with
            | Debug -> "Debug"
            | Release -> "Release"

        job { dotnet [ "publish"; Config.backendProject; "-c"; config ] }

    let serveWeb () =
        job {
            // Download server
            if not (File.Exists Config.serverArchivePath) then
                cmd
                    "wget"
                    [
                        "--no-cache"
                        "-O"
                        Config.serverArchivePath
                        "https://github.com/CompleteInformation/Core/releases/download/latest/CompleteInformation.tar.lz"
                    ]
            // Unpack server
            if not (Directory.Exists Config.serverPath) then
                Directory.CreateDirectory Config.serverPath |> ignore

                cmd "tar" [ "-xvf"; Config.serverArchivePath; "--directory"; Config.serverPath ]
            // Copy plugin into server
            Shell.mkdir $"{Config.serverPath}/plugins"
            Shell.mkdir $"{Config.serverPath}/WebRoot/plugins"

            let backendPlugin =
                $"{Path.GetDirectoryName(Config.backendProject)}/bin/Release/net6.0/publish/"

            let backendPluginPath = $"{Config.serverPath}/plugins/housekeeping/"
            Shell.mkdir backendPluginPath

            Directory.EnumerateFiles(backendPlugin, "Housekeeping.*")
            |> Seq.iter (Shell.copyFile $"{Config.serverPath}/plugins/housekeeping/")

            let frontendPluginPath = $"{Config.serverPath}/WebRoot/plugins/housekeeping/"
            Shell.mkdir frontendPluginPath

            // Start server
            parallelJob {
                CreateProcess.fromRawCommand $"{Config.serverPath}/CompleteInformation.App" []
                |> CreateProcess.withWorkingDirectory Config.serverPath
                |> Job.fromCreateProcess

                CreateProcess.fromRawCommand "npm" [ "run"; "watch" ]
                |> CreateProcess.withWorkingDirectory "./frontend/web"
                |> Job.fromCreateProcess
            }
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
        | [ "serve"; "web" ]
        | [ "serve" ]
        | [] ->
            job {
                Task.restore ()
                Task.build Release
                Task.publish Release
                Task.serveWeb ()
            }
        | _ -> Job.error [ "Usage: dotnet run [<command>]"; "Look up available commands in run.fs" ]
    |> Job.execute
