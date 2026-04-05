namespace EventModeling

/// Given/When/Then for a CommandSlice.
///
/// Carries concrete example data — this record is simultaneously:
///   - the business specification a stakeholder can verify
///   - the test case a developer derives implementation from
///
/// Given: prior Event boxes (orange) — state the CommandHandler reads from
/// When:  the Command box (blue) being applied
/// Then:  the resulting Event box(es) (orange) produced
type CommandGWT<'TState, 'TCommand, 'TEvent> = {
    Given: Event<'TState> list
    When:  Command<'TCommand>
    Then:  Event<'TEvent> list
}

/// Given/When/Then for a ViewSlice.
///
/// Carries concrete example data — this record is simultaneously:
///   - the business specification a stakeholder can verify
///   - the test case a developer derives implementation from
///
/// Given: Event boxes (orange) the EventHandler reads from
/// When:  the filter / sort / transform criteria applied
/// Then:  the View box (green) produced
type ViewGWT<'TEvent, 'TCriteria, 'TView> = {
    Given: Event<'TEvent> list
    When:  'TCriteria
    Then:  View<'TView>
}
