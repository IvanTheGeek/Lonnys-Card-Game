module EventModeling.Tests.Tests

open System
open Expecto
open EventModeling
open EventModeling.Testing.Assert
open EventModeling.Testing.GroupingRunner

// ─── Sample domain types ─────────────────────────────────────────────────────
// Minimal card game types used to exercise the GWT adapter.

type CardDrawn     = { Card: string; DrawnBy: string }
type DrawCard      = { PlayerName: string }
type DeckShuffled  = { ShuffledWithSeed: int }
type ShuffleDeck   = { RequestedSeed: int }
type HandCriteria  = { PlayerName: string }
type HandView      = { Cards: string list }

// ─── Actors ──────────────────────────────────────────────────────────────────

let dealer = { Name = "Dealer"; Kind = Automation "dealer"; Swimlane = None }
let player1 = { Name = "Player 1"; Kind = Human "player"; Swimlane = Some "Players" }

// ─── Sample CommandSlice — Shuffle Deck ──────────────────────────────────────

let shuffleDeckSlice : CommandSlice<Actor, DeckShuffled, ShuffleDeck, DeckShuffled> =
    let handler (given: Event<DeckShuffled> list) (cmd: Command<ShuffleDeck>) =
        Ok [ { Name = "DeckShuffled"; OccurredAt = DateTimeOffset.UtcNow; Data = { ShuffledWithSeed = cmd.Data.RequestedSeed } } ]

    { Actor   = dealer
      Command = { Name = "ShuffleDeck"; IssuedBy = dealer; Data = { RequestedSeed = 42 } }
      Handler = handler
      GWT =
        { Given = []
          When  = { Name = "ShuffleDeck"; IssuedBy = dealer; Data = { RequestedSeed = 42 } }
          Then  = [ { Name = "DeckShuffled"; OccurredAt = DateTimeOffset.MinValue; Data = { ShuffledWithSeed = 42 } } ] } }

// ─── Sample ViewSlice — Player Views Hand ────────────────────────────────────

let playerHandSlice : ViewSlice<CardDrawn, HandCriteria, HandView, Actor> =
    let handler (events: Event<CardDrawn> list) (criteria: HandCriteria) =
        let cards =
            events
            |> List.filter (fun e -> e.Data.DrawnBy = criteria.PlayerName)
            |> List.map (fun e -> e.Data.Card)
        { Name = "PlayerHand"; Data = { Cards = cards } }

    let drawnEvent card = { Name = "CardDrawn"; OccurredAt = DateTimeOffset.MinValue; Data = { Card = card; DrawnBy = "Player 1" } }

    { Events  = []
      View    = { Name = "PlayerHand"; Data = { Cards = [] } }
      Handler = handler
      GWT =
        { Given = [ drawnEvent "Ace of Spades"; drawnEvent "King of Hearts" ]
          When  = { PlayerName = "Player 1" }
          Then  = { Name = "PlayerHand"; Data = { Cards = [ "Ace of Spades"; "King of Hearts" ] } } }
      Actor   = player1 }

// ─── Registry and Grouping ───────────────────────────────────────────────────

let shuffleRef = CommandRef("ShuffleDeck", dealer)
let handRef    = ViewRef("PlayerHand", player1)

let registry : SliceRegistry =
    [ shuffleRef, CommandEntry (commandSliceToTest "shuffle deck"      shuffleDeckSlice)
      handRef,    ViewEntry    (viewSliceToTest    "player views hand"  playerHandSlice) ]
    |> Map.ofList

let cardGameGrouping =
    Area("Card Game",
        [ Workflow("Setup",
            [ Flow("Deal", [ shuffleRef ]) ])
          Workflow("Gameplay",
            [ Flow("Hand", [ handRef ]) ]) ])

let allTests = groupingToTest registry cardGameGrouping
