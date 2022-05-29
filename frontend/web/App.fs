namespace CompleteInformation.Plugins.Housekeeping.Frontend.Web

open Elmish
open Elmish.React

open CompleteInformation.Base.Frontend.Web

module App =
    let activate () =
        Program.mkProgram Index.init Index.update Index.view
#if DEBUG
        |> Program.withConsoleTrace
#endif
        |> Program.mount
        |> Program.run
