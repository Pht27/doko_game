# Software Architecture

Clean Architecture with three layers. Strict dependency rule: **Domain ← Application ← Api**.

No layer may reference a layer above it. The Domain layer has zero external dependencies.

---

## Layer Overview

```
Doko.Domain        — Pure game model. No I/O, no frameworks.
Doko.Application   — Use cases and game orchestration. Depends only on Domain.
Doko.Api           — Thin ASP.NET Core shell. Depends on Application.
```

---

## Domain Layer: `Doko.Domain`

### `Doko.Domain.Cards`

The physical building blocks of the game.

| Type | Kind | Description |
|---|---|---|
| `Suit` | enum | `Kreuz \| Pik \| Herz \| Karo` |
| `Rank` | enum | `Nine \| Jack \| Queen \| King \| Ten \| Ace` |
| `CardType` | record | `(Suit Suit, Rank Rank)` — one of 24 logical card identities |
| `Card` | record | `(CardId Id, CardType Type)` — physical card instance (two per type) |
| `CardId` | value type | `byte` 0–47, distinguishes the two copies of the same `CardType` |
| `CardPoints` | static class | `static int Of(Rank rank)` — A=11, 10=10, K=4, Q=3, J=2, 9=0 |
| `Deck` | static class | `Standard48()`, `Standard40()` (no Nines) — returns `IReadOnlyList<Card>` |

---

### `Doko.Domain.Trump`

Trump status and ranking are **not intrinsic to a card** — they depend on the active game mode. Everything goes through `ITrumpEvaluator`, resolved once at reservation time and stored on `GameState`.

#### Interface

```csharp
ITrumpEvaluator
  bool IsTrump(CardType card)
  int GetTrumpRank(CardType card)   // higher int = stronger trump
  int GetPlainRank(CardType card)   // used when the led suit is plain
```

#### Base Implementations

| Class | Description |
|---|---|
| `StandardTrumpEvaluator` | Normal Doppelkopf: ♥10 > ♣Q > ♠Q > ♥Q > ♦Q > ♣J > ♠J > ♥J > ♦J > ♦A > ♦K > ♦10 > ♦9 |
| `FarbsoloTrumpEvaluator(Suit soloSuit)` | Replaces the ♦ plain suit with `soloSuit`; same Queen/Jack structure |
| `DamensoloTrumpEvaluator` | Only Queens are trump (♣Q > ♠Q > ♥Q > ♦Q) |
| `BubensoloTrumpEvaluator` | Only Jacks are trump (♣J > ♠J > ♥J > ♦J) |
| `FleischlosesEvaluator` | No trump at all |
| `KnochenlosesEvaluator` | No trump; lowest trick wins (inversion handled via separate flag on `GameState`) |
| `SchlankerMartinEvaluator` | No trump; fewest tricks wins (tie-breaking inverted) |

#### Decorator for Sonderkarten Ranking Overrides

```csharp
SonderkarteRankingDecorator(ITrumpEvaluator inner, IReadOnlyList<ISonderkarteRankingModifier> modifiers)
```

Wraps any base evaluator and applies active ranking modifiers in priority order. The active modifiers are determined by `GameState.ActiveSonderkarten`.

#### Interface: `ISonderkarteRankingModifier`

```csharp
ISonderkarteRankingModifier
  bool Applies(GameState state)      // checks state.ActiveSonderkarten
  int ModifyRank(CardType card, int currentRank)
```

| Implementation | Effect |
|---|---|
| `SchweinchModifier` | Both ♦A held → ♦A ranks above Dulle |
| `SuperschweinchModifier` | Both ♦10 held → ♦10 ranks above Schweinchen; requires `Schweinchen` in `ActiveSonderkarten` |
| `HyperschweinchModifier` | Both ♦K held → ♦K ranks above Superschweinchen; requires `Superschweinchen` in `ActiveSonderkarten` |
| `HeidmannModifier` | Both ♠J held → all Jacks rank above all Queens |
| `HeidfrauModifier` | Both ♠Q held → all Queens rank above all Jacks (counters Heidmann) |

---

### `Doko.Domain.Hands`

| Type | Kind | Description |
|---|---|---|
| `Hand` | class | `IReadOnlyList<Card> Cards`; `bool Contains(Card)`; `Hand Remove(Card)` |

---

### `Doko.Domain.Tricks`

| Type | Kind | Description |
|---|---|---|
| `TrickCard` | record | `(Card Card, PlayerId Player)` — one played card with its owner |
| `Trick` | class | Ordered list of up to 4 `TrickCard`s; `PlayerId? Winner(ITrumpEvaluator)`; `int Points`; `bool IsComplete` |

`Trick.Winner` applies the following resolution rules:
1. If any trump was played, highest trump wins.
2. Otherwise, highest card of the led suit wins.
3. Dulle tie-breaking is delegated to `RuleSet.DulleRule` (accessed via `GameState.Rules`).

---

### `Doko.Domain.Players`

`PlayerSeat` is a **fixed, immutable** table position assigned at game creation. It never changes during the game. Play order is not determined by seat — it is determined by `GameState.CurrentTurn` and `GameState.Direction` (see `GameFlow`).

| Type | Kind | Description |
|---|---|---|
| `PlayerId` | value type | `byte` 0–3 |
| `PlayerSeat` | enum | `First \| Second \| Third \| Fourth` — static table position, never changes |
| `PlayerState` | record | `(PlayerId Id, PlayerSeat Seat, Hand Hand, Party? KnownParty)` |

`KnownParty` is `null` until the party is publicly revealed (e.g. by playing ♣Q or via announcement).

---

### `Doko.Domain.Parties`

Party membership can change mid-game (Hochzeit, Genscher, Schwarze Sau). It is resolved through a **decorator chain** assembled at reservation time.

| Type | Kind | Description |
|---|---|---|
| `Party` | enum | `Re \| Kontra` |

#### Interface

```csharp
IPartyResolver
  Party? ResolveParty(PlayerId player, GameState state)
  bool IsFullyResolved(GameState state)   // true when all 4 parties are known
```

#### Implementations

| Class | Description |
|---|---|
| `QueenOfClubsPartyResolver` | Holding ♣Q → Re; others → Kontra. Used for standard games. |
| `HochzeitPartyResolver(PlayerId hochzeitPlayer)` | Until the marrying trick: `hochzeitPlayer` → Re, all others → Kontra. After the first trick that `hochzeitPlayer` does not win: the winner of that trick joins Re; the other two stay Kontra. If the game ends with no marrying trick (Hochzeit player won every trick), scoring treats it as a solo. Tracks `bool MarriageRevealed` and `PlayerId? RevealedPartner` internally. |
| `SchwarzeSauPartyResolver(PlayerId schwarzeSauPlayer)` | Before the deciding trick: `schwarzeSauPlayer` → Re, all others → Kontra. At the deciding trick: the player who wins that trick becomes Re; all others (including `schwarzeSauPlayer`) become Kontra. The deciding trick is the last trick that determines whether the Schwarze Sau player achieves their goal (definition to be specified in game-rules.md). |
| `GenscherPartyResolver(IPartyResolver inner)` | Applies Genscherdamen partner swap; wraps inner resolver. |
| `GegengenscherPartyResolver(IPartyResolver inner)` | Counters a Genscher swap; wraps inner resolver. |

---

### `Doko.Domain.Reservations`

A reservation is declared before play begins. The highest-priority reservation wins; ties go to the first declarer (counterclockwise order).

**Schwarze Sau is not a reservation.** It is a fallback game mode that activates only when an Armut is declared and subsequently declined by all other players (see `ArmutReservation.OnDeclined()`).

| Type | Kind | Description |
|---|---|---|
| `ReservationPriority` | enum | `Solo = 0 \| Armut = 1 \| Hochzeit = 2 \| Schmeissen = 3` |

#### Interface

```csharp
IReservation
  ReservationPriority Priority { get; }
  bool IsEligible(Hand hand, RuleSet rules)
  GameModeContext Apply(GameState state)   // returns updated trump evaluator + party resolver
```

#### Implementations

| Class | Description |
|---|---|
| `ArmutReservation(PlayerId player)` | ≤3 trump cards excluding Füchse; player offers trump cards for exchange. If all others decline: `OnDeclined()` returns a `SchwarzeSauGameMode` which activates `SchwarzeSauPartyResolver`. |
| `HochzeitReservation(PlayerId player)` | Holding both ♣Q; activates `HochzeitPartyResolver` |
| `SchmeissenReservation(PlayerId player)` | Triggers redeal |

---

### `Doko.Domain.Announcements`

| Type | Kind | Description |
|---|---|---|
| `AnnouncementType` | enum | `Re \| Kontra \| Keine90 \| Keine60 \| Keine30 \| Schwarz` |
| `Announcement` | record | `(PlayerId Player, AnnouncementType Type, int TrickNumber, int CardIndexInTrick)` |

#### Domain Service

```csharp
AnnouncementRules
  bool CanAnnounce(AnnouncementType type, PlayerId player, GameState state)
  bool IsMandatory(PlayerId player, GameState state)   // Pflichtansage
  bool ViolatesFeigheit(GameResult result, GameState state)
```

Rules access `state.Rules` directly — no need to pass `RuleSet` separately.

Timing rule: announcements are allowed until **before the second card of the second trick** is played (tracked via `TrickNumber` and `CardIndexInTrick` on `GameState`).

---

### `Doko.Domain.Sonderkarten`

Sonderkarten are triggered by a player holding **both copies** of a specific card type. Ranking-modifying Sonderkarten are handled in `Doko.Domain.Trump`; the types here cover **state-modifying** effects.

| Type | Kind | Description |
|---|---|---|
| `SonderkarteType` | enum | `Schweinchen \| Superschweinchen \| Hyperschweinchen \| LinksGehangter \| RechtsGehangter \| Genscherdamen \| Gegengenscherdamen \| Heidmann \| Heidfrau \| Kemmerich ` |

#### Sealed `GameStateModification` Hierarchy

`ISonderkarte.Apply()` returns a `GameStateModification` rather than directly mutating state. `GameState.Apply(GameStateModification)` is the only place that mutates itself, keeping `GameState` in control of its own invariants.

```csharp
abstract record GameStateModification

record ReverseDirectionModification()
  : GameStateModification

record WithdrawAnnouncementModification(PlayerId Player, AnnouncementType Type)
  : GameStateModification

record TransferCardPointsModification(CardType From, CardType To)
  : GameStateModification
```

#### Interface

```csharp
ISonderkarte
  SonderkarteType Type { get; }
  bool IsTriggered(Hand hand)                         // player holds both copies
  GameStateModification? Apply(GameState state)       // null if no state change needed
```

#### Implementations

| Class | Effect |
|---|---|
| `GehangterSonderkarte(bool leftward)` | Returns `ReverseDirectionModification` |
| `KemmerichSonderkarte` | Returns `WithdrawAnnouncementModification` for an announcement of the holder's choice |

#### Registry

```csharp
SonderkarteRegistry
  IReadOnlyList<ISonderkarte> GetActive(RuleSet rules, GameState state)
```

---

### `Doko.Domain.Extrapunkte`

Bonus/penalty points evaluated after each completed trick. An `ExtrapunktAward` is a record of one bonus being granted: which bonus fired, who benefits, and the point delta (±1).

| Type | Kind | Description |
|---|---|---|
| `ExtrapunktType` | enum | `Doppelkopf \| FuchsGefangen \| Karlchen \| Agathe \| Fischauge \| GansGefangen \| Festmahl \| Blutbad \| Klabautermann \| Kaffeekranzchen` |
| `ExtrapunktAward` | record | `(ExtrapunktType Type, PlayerId BenefittingPlayer, int Delta)` |

#### Interface

```csharp
IExtrapunkt
  ExtrapunktType Type { get; }
  IReadOnlyList<ExtrapunktAward> Evaluate(Trick completedTrick, GameState state)
```

#### Implementations

| Class | Condition |
|---|---|
| `DoppelkopfExtrapunkt` | Trick value ≥ 40 points |
| `FuchsGefangenExtrapunkt` | Opposing party captures ♦A (Fuchs) |
| `KarlchenExtrapunkt` | ♣J wins the last trick |
| `AgathExtrapunkt` | ♦Q beats ♣J in the last trick |
| `FischaugeExtrapunkt` | Both ♦9s win tricks after first trump was played |
| `GansGefangenExtrapunkt` | ♦A captures ♦9 (Gans) |
| `FestmahlExtrapunkt` | Trick contains ≥3 Füchse (♦A) |
| `BlutbadExtrapunkt` | Trick contains all four Aces |
| `KlabautermannExtrapunkt` | ♠Q captures ♠K |
| `KaffeekranzhenExtrapunkt` | All four Queens appear in a single trick |

#### Registry

```csharp
ExtrapunktRegistry
  IReadOnlyList<IExtrapunkt> GetActive(RuleSet rules)
```

---

### `Doko.Domain.Scoring`

| Type | Kind | Description |
|---|---|---|
| `TrickResult` | record | `(Trick Trick, PlayerId Winner, IReadOnlyList<ExtrapunktAward> Awards)` |
| `CompletedGame` | record | `(GameState FinalState, IReadOnlyList<TrickResult> Tricks)` |
| `GameResult` | record | `(Party Winner, int ReAugen, int KontraAugen, int GameValue, IReadOnlyList<ExtrapunktAward> AllAwards, bool Feigheit)` |

#### Interface and Default Implementation

```csharp
IGameScorer
  GameResult Score(CompletedGame game)
```

`RuleSet` is provided at construction time (injected when the game starts), not per `Score()` call, since a game's rules never change mid-game.

```csharp
StandardGameScorer(RuleSet rules) : IGameScorer
```

**Scoring formula:**

| Component | Value |
|---|---|
| Gewonnen (win) | +1 |
| Gegen die Alten (Kontra wins) | +1 |
| Keine 90 achieved | +1 |
| Keine 60 achieved | +1 |
| Keine 30 achieved | +1 |
| Schwarz achieved | +1 |
| Per announcement made by losing party | +1 each |
| Extrapunkte | ±1 per award |
| Solo multiplier | total × 3 |

---

### `Doko.Domain.GameFlow`

The central aggregate root. All game state flows through here. `GameState` is the only type that mutates itself — via `GameState.Apply(GameStateModification)` and internal methods called by the Application layer.

| Type | Kind | Description |
|---|---|---|
| `GamePhase` | enum | `Dealing \| Reservations \| Playing \| Scoring \| Finished` |
| `PlayDirection` | enum | `Counterclockwise \| Clockwise` |
| `GameId` | value type | `Guid` wrapper |

#### Aggregate Root: `GameState`

```csharp
GameState
  GameId Id
  GamePhase Phase
  RuleSet Rules                              // immutable; set once at game creation
  IReadOnlyList<PlayerState> Players
  PlayerId CurrentTurn                       // who plays next
  PlayDirection Direction                    // affects NextPlayer()
  IReservation? ActiveReservation
  IReadOnlyList<Trick> CompletedTricks
  Trick? CurrentTrick
  IReadOnlyList<Announcement> Announcements
  IReadOnlyList<SonderkarteType> ActiveSonderkarten   // triggered Sonderkarten
  ITrumpEvaluator TrumpEvaluator             // resolved after reservations phase
  IPartyResolver PartyResolver               // composed decorator chain

  PlayerId NextPlayer(PlayerId current, PlayDirection direction)
  void Apply(GameStateModification modification)
```

`NextPlayer` is a pure function of the current player, the seat layout, and the active direction. Flipping `Direction` (via `ReverseDirectionModification`) automatically changes who plays next without any special-casing.

> **Note on `RuleSet` serialization:** Loading a `RuleSet` from a JSON or other config file is an Infrastructure concern and is out of scope for the Domain. The `RuleSet` record is a plain immutable C# record; deserialization from config happens in the Application or Infrastructure layer.

#### Domain Events

Raised by `GameState` mutations; consumed by the Application layer.

| Event | Raised when |
|---|---|
| `CardPlayedEvent` | A card is added to the current trick |
| `TrickCompletedEvent` | The 4th card is played and a winner is determined |
| `AnnouncementMadeEvent` | A player announces Re/Kontra/Keine90/etc. |
| `ReservationMadeEvent` | A player declares a reservation |
| `PartyRevealedEvent` | A party assignment becomes publicly known |
| `SonderkarteTriggeredEvent` | A Sonderkarte is activated and added to `ActiveSonderkarten` |

---

### `Doko.Domain.Rules`

Single immutable configuration record. Set once at game creation and stored on `GameState.Rules`. Domain services access rules via `state.Rules` — no need to pass `RuleSet` separately into method calls.

Serialization to/from JSON (or other formats) is handled outside the Domain.

```csharp
RuleSet   // immutable record

  // Deck
  bool PlayWithNines

  // Game modes
  bool AllowFarbsoli
  bool AllowDamensolo
  bool AllowBubensolo
  bool AllowFleischloses
  bool AllowNullo
  bool AllowKnochenloses
  bool AllowSchlankerMartin
  bool AllowStilleSolo
  bool AllowArmut
  bool AllowSchwarzeSau
  bool AllowHochzeit
  bool AllowSchmeissen

  // Dulle tie-break variant
  DulleRule DulleRule          // SecondBeatsFirst | FirstBeatsSecond

  // Sonderkarten (each independently toggleable)
  bool EnableSchweinchen
  bool EnableSuperschweinchen
  bool EnableHyperschweinchen
  bool EnableGehangter
  bool EnableGenscherdamen
  bool EnableGegengenscherdamen
  bool EnableHeidmann
  bool EnableHeidfrau
  bool EnableKemmerich

  // Announcement mechanics
  bool AllowAnnouncements
  bool EnforceFeigheit
  bool EnforcePflichtansage

  // Extrapunkte (each independently toggleable)
  bool EnableDoppelkopf
  bool EnableFuchsGefangen
  bool EnableKarlchen
  bool EnableAgathe
  bool EnableFischauge
  bool EnableGansGefangen
  bool EnableFestmahl
  bool EnableBlutbad
  bool EnableKlabautermann
  bool EnableKaffeekranzchen

  static RuleSet Default()     // standard Koppeldopf rules
  static RuleSet Minimal()     // bare-minimum, all optional rules off
```

| Type | Kind | Description |
|---|---|---|
| `DulleRule` | enum | `SecondBeatsFirst \| FirstBeatsSecond` |

---

## Application Layer: `Doko.Application`

### `Doko.Application.Games`

Orchestrates game flow. Depends only on `Doko.Domain`.

#### Commands

| Type | Fields |
|---|---|
| `StartGameCommand` | `IReadOnlyList<PlayerId> Players, RuleSet Rules` |
| `DealCardsCommand` | `GameId GameId` |
| `MakeReservationCommand` | `GameId GameId, PlayerId Player, IReservation? Reservation` |
| `PlayCardCommand` | `GameId GameId, PlayerId Player, CardId Card` |
| `MakeAnnouncementCommand` | `GameId GameId, PlayerId Player, AnnouncementType Type` |

#### Results

| Type | Fields |
|---|---|
| `StartGameResult` | `GameId GameId` |
| `PlayCardResult` | `TrickResult? CompletedTrick` — non-null when trick just finished |
| `GameFinishedResult` | `GameResult Result` |

#### Use Case Services

| Class | Responsibility |
|---|---|
| `StartGameUseCase` | Creates `GameState`, deals cards, stores via `IGameRepository` |
| `DealCardsUseCase` | Shuffles deck, distributes hands, advances phase to `Reservations` |
| `MakeReservationUseCase` | Collects all reservations, resolves highest-priority, builds `ITrumpEvaluator` + `IPartyResolver` chain; handles Armut decline → Schwarze Sau transition |
| `PlayCardUseCase` | Validates legal play (must follow suit if possible), adds to trick, triggers `TrickCompletedEvent` if trick is complete, checks for `Pflichtansage` |
| `MakeAnnouncementUseCase` | Validates timing and eligibility via `AnnouncementRules`, records `Announcement` |
| `FinishGameUseCase` | Invokes `IGameScorer`, stores `GameResult` |

#### Repository Interface

Defined in Application; implemented in Infrastructure (future).

```csharp
IGameRepository
  Task<GameState?> GetAsync(GameId id)
  Task SaveAsync(GameState state)
```

---

## API Layer: `Doko.Api`

Thin ASP.NET Core shell. No game logic here.

### Controllers

| Class | Routes |
|---|---|
| `GamesController` | `POST /games` — start game |
| | `POST /games/{id}/deal` — deal cards |
| | `POST /games/{id}/reservations` — submit reservation |
| | `POST /games/{id}/cards` — play a card |
| | `POST /games/{id}/announcements` — make an announcement |
| | `GET /games/{id}` — get current game state (read model) |

### DTOs

JSON-serializable records mirroring the Application command/result types. Mapped in controller action methods; no domain types leak into the API surface.

---

## Dependency Diagram

```
Doko.Api
  └── Doko.Application
        └── Doko.Domain
              └── (no dependencies)
```

Registries (`SonderkarteRegistry`, `ExtrapunktRegistry`) are constructed by `MakeReservationUseCase` and `PlayCardUseCase` respectively, consulting `state.Rules` to filter to only the active rules.
