module EventModeling.Testing.GroupingRunner

open Expecto
open EventModeling

/// A pre-built Expecto Test derived from a slice.
/// Type parameters are erased at this boundary — slices are converted to Test values
/// at the typed call site (where generic params are known) and stored here.
/// This avoids obj boxing or reflection while keeping the registry heterogeneous.
type SliceTestEntry =
    | CommandEntry of Test
    | ViewEntry    of Test

/// Maps SliceRef keys to pre-built Expecto Test values.
/// Build this with Assert.commandSliceToTest / Assert.viewSliceToTest
/// before calling groupingToTest.
type SliceRegistry = Map<SliceRef, SliceTestEntry>

let private entryToTest (entry: SliceTestEntry) : Test =
    match entry with
    | CommandEntry t -> t
    | ViewEntry    t -> t

let private refToTest (registry: SliceRegistry) (ref: SliceRef) : Test =
    match Map.tryFind ref registry with
    | Some entry -> entryToTest entry
    | None ->
        let name =
            match ref with
            | CommandRef(name, _) -> $"CommandRef({name})"
            | ViewRef(name, _)    -> $"ViewRef({name})"
        testCase $"MISSING SLICE: {name}" <| fun () ->
            failtest $"SliceRef not found in registry: {name}"

/// Recursively maps the Grouping hierarchy to a nested Expecto testList tree.
/// Flow     → testList containing slice tests
/// Workflow → testList containing child Grouping tests (recursive, no depth limit)
/// Area     → testList containing Workflow tests (recursive)
///
/// The output mirrors the Event Model grid structure:
///   Area > Workflow > Flow > test case
///
/// Missing registry entries produce a named failing test — the tree always runs fully.
let rec groupingToTest (registry: SliceRegistry) (grouping: Grouping) : Test =
    match grouping with
    | Flow(name, sliceRefs) ->
        testList name (List.map (refToTest registry) sliceRefs)
    | Workflow(name, children) ->
        testList name (List.map (groupingToTest registry) children)
    | Area(name, workflows) ->
        testList name (List.map (groupingToTest registry) workflows)
