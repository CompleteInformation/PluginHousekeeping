namespace CompleteInformation.Plugins.Housekeeping.Frontend.Web

open Elmish
open Elmish.React

#if DEBUG
open Elmish.HMR
#endif

module App =
    let activate () =
        Program.mkProgram Index.init Index.update Index.view
#if DEBUG
        |> Program.withConsoleTrace
#endif
        |> Program.withReactSynchronous "module-slot"
        |> Program.run
