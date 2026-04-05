module EventModeling.Tests.Program

open Expecto
open EventModeling.Tests.Tests

[<EntryPoint>]
let main args =
    runTestsWithCLIArgs [] args allTests
