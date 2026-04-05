namespace EventModeling

open System

/// The source of decisions and actions. Issues Commands and consumes Views.
type ActorKind =
    | Human          of role: string
    | Automation     of name: string
    | ExternalSystem of name: string

type Actor = {
    Name:     string
    Kind:     ActorKind
    Swimlane: string option
}

/// Orange box. An immutable fact that has occurred. Named in past tense.
/// The backbone of the timeline and the only coupling point in the system.
/// Never changes — only accumulates.
type Event<'T> = {
    Name:       string
    OccurredAt: DateTimeOffset
    Data:       'T
}

/// Blue box. Intent expressed by an Actor. Handled by a pure CommandHandler.
type Command<'T> = {
    Name:     string
    IssuedBy: Actor
    Data:     'T
}

/// Green box. Shaped data produced by an EventHandler for an Actor to consume.
type View<'T> = {
    Name: string
    Data: 'T
}
