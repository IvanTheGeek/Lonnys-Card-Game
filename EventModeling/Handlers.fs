namespace EventModeling

/// Sits hidden behind the Command box. Never appears in the top-level flow.
/// Pure: same inputs always produce the same output. No side effects.
///
/// Given: prior Event boxes (orange) — the state derived from prior events
/// When:  the Command box (blue) being applied
/// Then:  the resulting Event box(es) (orange)
type CommandHandler<'TState, 'TCommand, 'TEvent> =
    Event<'TState> list -> Command<'TCommand> -> Result<Event<'TEvent> list, string>

/// Sits hidden behind the View box. Never appears in the top-level flow.
/// Pure: a data pipeline. F# |> piping made explicit —
///   events |> filter criteria |> transform |> shape into View
///
/// Given: Event boxes (orange) the handler reads from
/// When:  the filter / sort / transform criteria
/// Then:  the View box (green) produced
type EventHandler<'TEvent, 'TCriteria, 'TView> =
    Event<'TEvent> list -> 'TCriteria -> View<'TView>
