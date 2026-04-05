# Event Model — Lonny's Card Game

## Actors

| Actor | Type | Description |
|---|---|---|
| System | Automation | Handles deck operations and evaluation — no arbitrary decisions |
| Player 1 | Human | Makes decisions on keep/redraw |
| Player 2 | Human | Makes decisions on keep/redraw |

---

## PATH: One Complete Game Round (Happy Path)

The path traces one full round from deck shuffle through winner declaration and play-again decision.

Data flow direction per slice type:
- **CommandSlice** → top to bottom: Actor → Command → Event(s)
- **ViewSlice** → bottom to top: Event(s) ^ View ^ Actor

---

### SLICE 1 — CommandSlice: ShuffleDeck
```
Actor Row:    System
Cmd Row:      ShuffleDeck
Event Row:    DeckShuffled
```
**GWT (hidden):**
- Given: nothing (start of round)
- When: ShuffleDeck
- Then: DeckShuffled — 52 unique card positions assigned randomly, no duplicates

---

### SLICE 2 — CommandSlice: DealToPlayer1
```
Actor Row:    System  [triggered by DeckShuffled]
Cmd Row:      DealCards(Player1)
Event Row:    CardsDealt(Player1, card1..card5)
```
**GWT (hidden):**
- Given: DeckShuffled, used[] tracking
- When: DealCards(Player1)
- Then: CardsDealt — 5 unique card positions drawn and marked used

> **Bug surfaces here:** Card 1 drawn without checking used[] — potential duplicate on first draw

---

### SLICE 3 — ViewSlice: Player1ViewsHand
```
Event Row:    CardsDealt(Player1)
View Row:     Player1Hand — 5 cards sorted descending by face value
Actor Row:    Player 1
```
**GWT (hidden):**
- Given: CardsDealt(Player1)
- When: (no filter — show all 5)
- Then: Player1Hand — sorted card list with suit and face name

---

### SLICE 4 — CommandSlice: DealToPlayer2
```
Actor Row:    System  [triggered by CardsDealt(Player1)]
Cmd Row:      DealCards(Player2)
Event Row:    CardsDealt(Player2, card1..card5)
```
**GWT (hidden):**
- Given: DeckShuffled, used[] tracking (Player1's cards already marked)
- When: DealCards(Player2)
- Then: CardsDealt — 5 more unique positions drawn and marked used

---

### SLICE 5 — ViewSlice: Player2ViewsHand
```
Event Row:    CardsDealt(Player2)
View Row:     Player2Hand — 5 cards sorted descending by face value
Actor Row:    Player 2
```
**GWT (hidden):**
- Given: CardsDealt(Player2)
- When: (no filter — show all 5)
- Then: Player2Hand — sorted card list with suit and face name

> **Design issue surfaces here:** Player 2's hand is shown on screen BEFORE Player 1 is asked about redrawing. Player 1 can see Player 2's cards. This is a sequencing flaw — in the code, both players are dealt before either player acts.

---

### SLICE 6 — CommandSlice: Player1DecidesDraw
```
Actor Row:    Player 1  [informed by Player1Hand view]
Cmd Row:      KeepHand(Player1)  |  RequestRedraw(Player1)
Event Row:    HandKept(Player1)  |  RedrawRequested(Player1)
```
**GWT (hidden):**
- Given: CardsDealt(Player1)
- When: input 'y' → KeepHand | input 'n' → RequestRedraw
- Then: HandKept OR RedrawRequested

---

### SLICE 7 — ViewSlice: Player1HandForRedrawSelection
*(only active when RedrawRequested(Player1) exists)*
```
Event Row:    RedrawRequested(Player1), CardsDealt(Player1)
View Row:     Player1HandNumbered — cards listed 1–5 with positions
Actor Row:    Player 1
```
**GWT (hidden):**
- Given: RedrawRequested(Player1), CardsDealt(Player1)
- When: (no filter — show all 5 with position numbers)
- Then: Player1HandNumbered — numbered list for selection

---

### SLICE 8 — CommandSlice: Player1ReplacesCard
*(repeating — once per card replaced)*
```
Actor Row:    Player 1  [informed by Player1HandNumbered view]
Cmd Row:      ReplaceCard(Player1, position)
Event Row:    CardReplaced(Player1, position, newCard)
```
**GWT (hidden):**
- Given: RedrawRequested(Player1), used[]
- When: ReplaceCard(position 1–5)
- Then: CardReplaced — new unique card drawn for that position, marked used

---

### SLICE 9 — ViewSlice: Player1UpdatedHand
*(follows each ReplaceCard — repeating)*
```
Event Row:    CardReplaced(Player1, ...)
View Row:     Player1Hand — refreshed, re-sorted
Actor Row:    Player 1
```
**GWT (hidden):**
- Given: CardsDealt(Player1), CardReplaced(Player1, ...) [all replacements so far]
- When: (no filter)
- Then: Player1Hand — current 5 cards re-sorted

> Slices 8 and 9 repeat until Player 1 enters any value outside 1–5

---

### SLICE 10 — CommandSlice: Player1FinishesRedraw
```
Actor Row:    Player 1
Cmd Row:      FinishRedraw(Player1)
Event Row:    HandFinalized(Player1)
```
**GWT (hidden):**
- Given: RedrawRequested(Player1), CardReplaced(Player1, ...) [zero or more]
- When: input outside 1–5
- Then: HandFinalized(Player1)

---

### SLICE 11 — ViewSlice: Player2HandReminder
*(displayplayer call in Lonny's code — shows Player 2's hand again before Player 2 acts)*
```
Event Row:    CardsDealt(Player2), CardReplaced(Player2, ...) [if any]
View Row:     Player2Hand — re-sorted
Actor Row:    Player 2
```
> **Redundant view surfaces here:** This is the second time Player 2's hand is shown. It exists because Player 2's hand was already displayed at deal time (Slice 5). This view is the same data, shown again. Suggests Lonny was working toward a cleaner flow but hasn't resolved the sequencing yet.

---

### SLICE 12 — CommandSlice: Player2DecidesDraw
```
Actor Row:    Player 2  [informed by Player2Hand view]
Cmd Row:      KeepHand(Player2)  |  RequestRedraw(Player2)
Event Row:    HandKept(Player2)  |  RedrawRequested(Player2)
```
*(same structure as Slice 6)*

---

### SLICES 13–15 — Player2 Redraw Loop
*(mirrors Slices 7–10 exactly for Player 2)*

- **Slice 13 — ViewSlice:** Player2HandForRedrawSelection
- **Slice 14 — CommandSlice:** Player2ReplacesCard *(repeating)*
- **Slice 15 — ViewSlice:** Player2UpdatedHand *(repeating)*
- **Slice 16 — CommandSlice:** Player2FinishesRedraw → HandFinalized(Player2)

---

### SLICE 17 — CommandSlice: EvaluatePlayer1Hand
```
Actor Row:    System  [triggered by HandFinalized(Player1)]
Cmd Row:      EvaluateHand(Player1)
Event Row:    HandEvaluated(Player1, handRank, points)
```
**GWT (hidden):**
- Given: CardsDealt(Player1), CardReplaced(Player1, ...) [all], HandFinalized(Player1)
- When: EvaluateHand(Player1)
- Then: HandEvaluated — hand rank detected (Royal Straight Flush → High Card), points calculated

> **Bug surfaces here:** Three-of-a-kind detection logic is incorrect — checking adjacent pairs does not reliably identify three of a kind. Also two pairs are not distinguished from one pair in the current point system.

---

### SLICE 18 — ViewSlice: Player1HandResult
```
Event Row:    HandEvaluated(Player1)
View Row:     Player1Result — hand rank label + points
Actor Row:    Player 1
```

---

### SLICE 19 — CommandSlice: EvaluatePlayer2Hand
```
Actor Row:    System  [triggered by HandFinalized(Player2)]
Cmd Row:      EvaluateHand(Player2)
Event Row:    HandEvaluated(Player2, handRank, points)
```

---

### SLICE 20 — ViewSlice: Player2HandResult
```
Event Row:    HandEvaluated(Player2)
View Row:     Player2Result — hand rank label + points
Actor Row:    Player 2
```

---

### SLICE 21 — CommandSlice: DeclareWinner
```
Actor Row:    System  [triggered by HandEvaluated(Player1) + HandEvaluated(Player2)]
Cmd Row:      DeclareWinner
Event Row:    WinnerDeclared(winner)
```
**GWT (hidden):**
- Given: HandEvaluated(Player1, points1), HandEvaluated(Player2, points2)
- When: DeclareWinner
- Then: WinnerDeclared — player with higher points wins

> **Bug surfaces here:** No tie handling. If points are equal, Player 2 is declared winner by default (else branch).

---

### SLICE 22 — ViewSlice: WinnerAnnouncement
```
Event Row:    WinnerDeclared(winner)
View Row:     WinnerScreen — "Player N WINS / Player N LOSES"
Actor Row:    Player 1, Player 2
```

---

### SLICE 23 — CommandSlice: PlayAgainDecision
```
Actor Row:    Player 1  (implicitly — single input, no role distinction)
Cmd Row:      PlayAgain  |  QuitGame
Event Row:    GameRestarted  |  GameEnded
```
**GWT (hidden):**
- Given: WinnerDeclared
- When: input read
- Then: GameRestarted OR GameEnded

> **Critical bug surfaces here:** `while (again = 'y')` uses assignment not comparison. GameEnded **never fires**. The game loops infinitely regardless of input. This means QuitGame is a missing command and GameEnded is a missing event in the actual implementation.

---

## Missing Events / Commands (Gaps the Model Exposes)

| Missing | Type | Impact |
|---|---|---|
| GameEnded | Event | Never produced — infinite loop bug |
| QuitGame | Command | Unreachable — assignment bug |
| TieDeclared | Event | No tie handling exists |
| DuplicateCardPrevented | Event | First card dealt to Player 1 skips used[] check |
| HandFinalized(Player1/2) | Event | Implied but not explicit — redraw exit is silent |

## Orphaned Code (Model Surfaces as Dead Paths)

| Element | Observation |
|---|---|
| `playagain()` function | Defined, never called — an entire alternate path that goes nowhere |
| `notshufflecards()` + ordered display | Produces events and views that no actor consumes — debug artifact with no path through it |
| `memory[5][14]` in main() | Declared, never used — state with no slice attached |
