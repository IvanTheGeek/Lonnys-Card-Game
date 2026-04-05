# Event Modeling

## Overview

Event Modeling is a methodology created by Adam Dymitruk for designing and documenting systems as a single, timeline-ordered visual blueprint. The artifact produced by this process is called an **Event Model**.

An Event Model captures a **PATH** — a specific, ordered flow of interactions and events through a system. There can be many paths through a system, but the practice focuses on walking through one path at a time to build shared understanding across all parties. The model breaks down into discrete, step-by-step slices that any stakeholder can read — a CEO, an accounts receivable clerk, a shipping coordinator, a developer, and a system architect can all look at the same model and extract what is meaningful to them.

---

## The Building Blocks

### Actors

The source of decisions and actions. An actor can be:

- A human user, admin, accounting clerk, or other role
- An automation, function, AI agent, or external system
- Anything capable of issuing a command

The key trait of an actor is that it can make an arbitrary or deterministic decision. The actor section may be subdivided into swimlanes representing different roles.

### Commands

Deterministic pure functions. The actor issues a command expressing intent. A hidden **Command Handler** sits behind the command and applies business rules using Given/When/Then (GWT):

- **Given** — prior event boxes (orange) that represent the state the handler reads from
- **When** — the command box (blue) being applied
- **Then** — the resulting event box(es) (orange) produced

The Command Handler is pure: same inputs always produce the same output. No side effects. The GWT enforces the business rules that gate whether and how an event is produced.

### Events

Facts that have occurred. Immutable. Written in past tense. Events are the **backbone of the timeline** and the **point of coupling** in the system. Once an event is produced, any number of View Slices can consume it at any point after it is created. Events are the only necessary coupling point — everything downstream reads from them.

### Views

The output of a View Slice. A View is produced by an **Event Handler** using GWT, but the GWT here serves a different purpose than in a Command Slice:

- **Given** — a set of event boxes (orange) that the handler reads from
- **When** — filtered, sorted, or transformed using specific criteria
- **Then** — a specific data shape is produced, represented as the view box (green)

This is analogous to F# data pipelining with `|>` — take a stream of events, pipe through transforms and filters, and produce the data shape the actor needs. The View then informs the actor, whether that is a UI screen, an API response, a processor, or any downstream consumer.

---

## The Slice

The fundamental unit of an Event Model is a **Slice**, which is one of two types:

### CommandSlice — State Change

```
Actor → Command → Event(s)
```

Data and control flow **top to bottom** through the grid. The **Command Handler** sits behind the Command (hidden) and uses the GWT rows to enforce business rules and produce the event(s).

### ViewSlice — State View

```
Event(s) → View → Actor
```

Data flows **bottom to top** — Event ^ View ^ Actor. The **Event Handler** sits behind the View (hidden) and uses the GWT rows to filter, sort, and shape the event data into the View.

---

## The Cadence

The Event Model flows as an alternating rhythm:

> CommandSlice → ViewSlice → CommandSlice → ViewSlice → ...

Like a sine wave. A CommandSlice produces a fact. A ViewSlice shapes that fact into something an actor can use. The actor then acts again. The opposing directions of data flow — top-down for commands, bottom-up for views — give the model this wave character.

---

## The Grid Layout

The Event Model is structured as a grid with four base row sections:

### Row 1 — Actor

The actor section. May be subdivided into swimlanes (sub-rows) for different roles:

- User, Admin, Shipping Clerk, Accounting Department, External System, etc.
- Can be a single row or many, depending on the complexity of the domain

### Row 2 — Command or View

A shared row. In a **CommandSlice** column it holds the **Command**. In a **ViewSlice** column it holds the **View**. Same row position, different content depending on the slice type of that column.

### Row 3 — Events

The immutable backbone of the timeline. May be subdivided into rows that align specific events with the department, system, or bounded context that owns them. Adam Dymitruk refers to this as the application of **Conway's Law** to the model — the structure of the event rows can mirror the structure of the teams or systems involved.

### Row 4 — GWT (Given / When / Then)

The Given/When/Then specification for the slice. Each G, W, and T may occupy its own row in this section.

### Header Rows (Row 0 and above) — Grouping and Hierarchy

As a model grows, slices are grouped into named units that span multiple columns. These groupings are rendered as labeled header rows **above** Row 1, with distinct background colors to visually separate them. They carry no logic — they are purely organizational and navigational.

A natural hierarchy emerges, with suggested terms for each level:

- **Flow** — the smallest named unit; a short sequence of slices for one focused interaction ("Login", "Add to Cart")
- **Workflow** — a named series of Flows or other Workflows that together accomplish a larger business process ("Checkout", "Order Fulfillment"). Workflows can be nested — a Workflow can contain other Workflows to represent progressively larger processes.
- **Area** — the top-level grouping; a named section of the application that organizes related Workflows ("Shopping", "Account Management")

The hierarchy is not fixed to three levels. A Workflow can contain Flows, other Workflows, or both. The depth is whatever the complexity of the system requires. Each grouping level gets its own header row and its own background color. The visual result is a set of nested color bands across the top of the grid — an at-a-glance map of the system for any reader.

**Terminology is intentionally minimal.** Starting with Flow, Workflow, and Area — three terms that both business and technical stakeholders can use naturally. Real usage will surface whether additional granularity (such as a "Task" level) is needed. Expand the vocabulary only when a genuine gap is felt, not in anticipation of one.

Row 4 is **typically visible** when the model is actively being designed and filled in — it is the working specification that feeds the handlers. What is **hidden** are the **Command Handler** and **Event Handler** themselves. These handlers are not shown in the top-level diagram flow; they are implied behind the Command and View respectively.

The standard visible flow in the model is:

- **CommandSlice**: Actor → Command → Event(s)
- **ViewSlice**: Event(s) → View → Actor

The GWT rows in Row 4 are critical because they define the exact logic the hidden handlers must implement. Without Row 4, the handlers have no specification to work from. The diagram remains readable to all audiences through the top-level flow; Row 4 provides the implementable detail beneath it.

---

## GWT — Purpose

The Given/When/Then pattern serves multiple purposes:

| Slice        | Given                                                         | When                                   | Then                                 |
| ------------ | ------------------------------------------------------------- | -------------------------------------- | ------------------------------------ |
| CommandSlice | Prior event boxes (orange) — the state the handler reads from | The command box (blue)                 | The resulting event box(es) (orange) |
| ViewSlice    | The relevant events represented as the orange boxes           | The filter / sort / transform criteria | The data shape (View) produced       |

In a CommandSlice, GWT enforces business rules and determines what events are emitted.
In a ViewSlice, GWT is a data pipeline — take events, apply transforms, produce a shaped output.

**GWT as tests and example data.** Along with Paths, GWT rows include concrete example data — real values filled into the Given, When, and Then boxes. This gives GWT a third role: the example data becomes the test suite on the technical side, and a human-readable reference for everyone else. A developer can read the GWT and derive the test directly from it. A business stakeholder can read the same GWT and verify the behavior matches their expectation. The model and the tests are the same artifact.

**Multiple Paths as a complete picture.** As more Paths are modeled, their GWT example data accumulates into a comprehensive set of tests and usage examples covering the full range of system behavior. This aggregate can be extracted directly to feed tutorial materials, onboarding documentation, and API usage guides — all grounded in the same example data that drives the tests. Nothing needs to be written twice.

---

## Slice Immutability and Independence

Decoupling is a primary tenant of event sourcing and Event Modeling. This applies directly to slices:

- **A finalized slice is immutable.** Once a slice is approved or goes to production, it is not changed. If the behavior must change, a new slice is created to replace it — the old one is not modified.
- **Slices are independent.** Code is not shared or reused across slices. Each slice owns its own logic entirely.
- **Copying is allowed, reuse is not.** A slice's code may be copied as a starting point for a new slice, but once copied the two are fully independent. There is no shared base, no abstraction, no common dependency between them.

This is what makes decoupling real. If slices shared code, a change to the shared code would affect all slices that depend on it — coupling them together through the back door. Independence guarantees that changing or replacing one slice has zero effect on any other.

---

## Key Principles

- **Events are immutable facts** — they never change, they only accumulate
- **Events are the only coupling point** — producers and consumers are decoupled; only the event schema matters
- **Commands are pure** — deterministic, no side effects, always produce the same output given the same input
- **Slices are immutable once finalized** — a slice is replaced, never modified; code is not shared between slices
- **The model is a PATH** — one specific flow through the system; multiple paths may exist and each can be modeled
- **The diagram serves all audiences** — business, design, and technical stakeholders read the same artifact at their own level of detail
- **GWT is the test suite** — example data in GWT rows doubles as tests for developers and a concrete reference for business stakeholders; the model and the tests are the same artifact
- **Handlers are hidden, GWT is not** — the Command Handler and Event Handler are not shown in the flow diagram; the GWT rows in Row 4 are visible when working the model and provide the specification those handlers implement
- **Conway's Law applies** — the event rows can reflect the organizational or system boundaries of the domain
