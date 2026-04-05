module EventModeling.Testing.Assert

open Expecto
open EventModeling

/// Converts a CommandSlice's GWT into an Expecto test.
/// Runs the hidden CommandHandler against the GWT example data and asserts the result.
/// Serves as the bridge between the Event Model specification and the test suite.
let commandSliceToTest
    (label: string)
    (slice: CommandSlice<'TActor, 'TState, 'TCommand, 'TEvent>)
    : Test =
    testCase label <| fun () ->
        let result = slice.Handler slice.GWT.Given slice.GWT.When
        Expect.isOk result $"CommandHandler returned Error for slice: {label}"
        let actual = Result.defaultValue [] result
        // OccurredAt is a runtime timestamp — not part of the business spec.
        // GWT Then specifies event Name and Data only.
        let strip (e: Event<'T>) = (e.Name, e.Data)
        Expect.equal (List.map strip actual) (List.map strip slice.GWT.Then) $"GWT Then mismatch for slice: {label}"

/// Converts a ViewSlice's GWT into an Expecto test.
/// Runs the hidden EventHandler against the GWT example data and asserts the result.
/// Serves as the bridge between the Event Model specification and the test suite.
let viewSliceToTest
    (label: string)
    (slice: ViewSlice<'TEvent, 'TCriteria, 'TView, 'TActor>)
    : Test =
    testCase label <| fun () ->
        let actual = slice.Handler slice.GWT.Given slice.GWT.When
        Expect.equal actual slice.GWT.Then $"GWT Then mismatch for slice: {label}"
