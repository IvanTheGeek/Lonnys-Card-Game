namespace EventModeling

/// CommandSlice — State Change.
/// Visible flow: Actor → Command → Event(s)   [top to bottom through the grid]
///
/// The CommandHandler is hidden behind the Command box.
/// The GWT rows define what the handler must implement.
///
/// Each slice is an independent, immutable unit.
/// Once finalized it is replaced — never modified.
/// No code is shared between slices; each owns its logic entirely.
type CommandSlice<'TActor, 'TState, 'TCommand, 'TEvent> = {
    Actor:   'TActor
    Command: Command<'TCommand>
    Handler: CommandHandler<'TState, 'TCommand, 'TEvent>
    GWT:     CommandGWT<'TState, 'TCommand, 'TEvent>
}

/// ViewSlice — State View.
/// Visible flow: Event(s) → View → Actor   [bottom to top through the grid]
///
/// The EventHandler is hidden behind the View box.
/// The GWT rows define what the handler must implement.
///
/// Each slice is an independent, immutable unit.
/// Once finalized it is replaced — never modified.
/// No code is shared between slices; each owns its logic entirely.
type ViewSlice<'TEvent, 'TCriteria, 'TView, 'TActor> = {
    Events:  Event<'TEvent> list
    View:    View<'TView>
    Handler: EventHandler<'TEvent, 'TCriteria, 'TView>
    GWT:     ViewGWT<'TEvent, 'TCriteria, 'TView>
    Actor:   'TActor
}

/// Structural reference to a slice by kind and name.
/// Paths and Groupings reference slices this way — maintaining decoupling.
/// Each slice remains a separate typed entity; referencing it does not
/// couple to its internal state, command, or event types.
type SliceRef =
    | CommandRef of name: string * actor: Actor
    | ViewRef    of name: string * actor: Actor
