namespace CompleteInformation.Plugins.Housekeeping.Frontend.Web

open Elmish
open Elmish.React

open CompleteInformation.Base.Frontend.Web
open CompleteInformation.Plugins.Housekeeping.Frontend.Web

module App =
    let activate () =
        Program.mkProgram State.init State.update View.render
#if DEBUG
        |> Program.withConsoleTrace
#endif
        |> Program.mount
        |> Program.run
