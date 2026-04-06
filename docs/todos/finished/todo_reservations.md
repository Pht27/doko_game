# Reservations

## Finding Reservations

The reservations in reality work a lot differently then they work currently. Here is a detailed breakdown of the process:

Every player, one after another in the playing order, gets asked whether they have a reservation ("Hast du einen Vorbehalt?" / "Bist du gesund?"). They either answer "Gesund" ("Kein Vorbehlat") or "Vorbehalt" / "Nicht gesund" without specifying which reservation they have (they also do not need to have decided on one yet! They will get asked for each reservation in order and eventually have to pick one, but can decide that at each point individually).

After all four players have been asked, the second phase commences: If only one player has a reservation, they will freely announce theirs and the game starts.

If not, the players get asked in order again if they have a Solo (not including Schlanker Martin!! That technically is a solo but it has the lowest reservation priority of all of them). When everyone has answered, all players that want to play a solo state their solo and the highest one wins (order should be defined in rules), where a tie favors the player who is first in playing order.

The players then get asked (in order again) if they have an Armut. If one or more has an Armut (favoring the first in play order), an Armut is being played (more about that later).

If noone has an Armut, the players get asked if anyone has the reservation "Schmeißen". If anyone has it, this reservation holds and the game is not played.

Then, the players get asked if they have a "Hochzeit". Hochzeit can only occur once, so if someone wants to play a Hochzeit, they play. Only after "getting" their Hochzeit, they have to decide on how they find their partner (First Trick, first trump trick etc...)

Lastly, if noone has a Hochzeit, the only remaining reservation is a "Schlanker Martin" and the first player in order gets to play their Schlanker Martin. At this point, they need to choose Schlanker Martin, because it is the last reservation remaining and they announced that they have a reservation.

## Finding Your Rich Partner

When the Armut is being played, the game advances to another special phase - the poor player has to find their rich partner. For this, the players sitting behind the poor player get asked in order, whether they accept the Armut.

If noone accepts the Armut, the game starts as game mode "Schwarze Sau" with the poor player playing the first card.

If someone accepts the Armut, the other players obviously do not get asked anymore. Then, the poor player gives all their trump cards to the rich player. They then freely choose as many cards as they recieved from their extended hand to return to the poor player (no restrictions at all, just the number of cards has to be the same).

When returning the cards, it is displayed, how many cards got returned and if they included trump (not specified which).

## Schwarze Sau

When in the game mode Schwarze Sau the trick with the second Pik Dame is concluded, the game enters another special phase (maybe it doesnt need to be a phase, but i have no other idea of how to handle it). The play who won that trick gets to choose a solo in which the game continues from here on. That includes every solo except Kontrasolo and Stille Hochzeit, but Schlanker Martin. The trump evaluators and card orders and so on immediately get swapped to that game modes configuration. The game in the end gets counted as if the player played this solo from the start.

## Idea

I think we need to make the game phases more dynamic as the reservation game phase can lead to more special game phases (Hochzeit, where the player chooses their finding criterion and Armut phases). Also, from the Schwarze Sau, the playing phase needs to be interrupted to choose a Solo.

---

## Implementation Plan

### Problem
The current reservation system is simplified: all 4 players declare simultaneously (in any order), and the server picks the highest-priority winner. Real Doppelkopf uses a sequential, phased discovery process.

### New Game Phases

Replace `GamePhase.Reservations` with a sequence of reservation sub-phases:

```
Dealing
  → ReservationHealthCheck     (each player declares Gesund/Vorbehalt)
    → 0 Vorbehalt              → Playing (normal game)
    → 1 Vorbehalt              → ReservationSoloCheck (player can declare any reservation)
    → 2+ Vorbehalt             → ReservationSoloCheck (only Solos offered)
      → Solo winner found      → Playing
      → No Solo                → ReservationArmutCheck
        → Armut found          → ArmutPartnerFinding
          → Partner found      → ArmutCardExchange → Playing
          → No partner         → Playing (Schwarze Sau mode)
        → No Armut             → ReservationSchmeissenCheck
          → Schmeißen found    → Geschmissen
          → No Schmeißen       → ReservationHochzeitCheck
            → Hochzeit found   → Playing
            → No Hochzeit      → Schlanker Martin (forced) → Playing
```

### Files Affected

**Domain**:
- `GamePhase.cs` — add `ReservationHealthCheck`, `ReservationSoloCheck`, `ReservationArmutCheck`, `ReservationSchmeissenCheck`, `ReservationHochzeitCheck`, `ArmutPartnerFinding`, `ArmutCardExchange`; remove `Reservations`
- `GameState.cs` — add `HealthDeclarations`, `PendingReservationResponders`, `ArmutPlayer`, `ArmutRichPlayer`
- `GameStateModification.cs` — add `RecordHealthDeclarationModification`, `SetPendingRespondersModification`, `ClearReservationDeclarationsModification`, `SetArmutPlayerModification`, `SetArmutRichPlayerModification`, `ArmutGiveTrumpsModification`

**Application**:
- `DealCardsUseCase.cs` — transition to `ReservationHealthCheck` (not `Reservations`)
- `MakeReservationUseCase.cs` — rework for phased check logic (validates allowed types per phase, uses `PendingReservationResponders`)
- New `DeclareHealthStatusUseCase.cs` — handles `ReservationHealthCheck`
- New `AcceptArmutUseCase.cs` — handles `ArmutPartnerFinding`
- New `ExchangeArmutCardsUseCase.cs` — handles `ArmutCardExchange`
- `GameQueryService.cs` — compute `EligibleReservations` per phase; add `ShouldDeclareHealth`, `ShouldRespondToArmut`, `ShouldReturnArmutCards`, `ArmutCardReturnCount` to view
- `PlayerGameView.cs` — new query fields
- `ServiceCollectionExtensions.cs` — register new use cases

**API**:
- `GamesController.cs` — new endpoints: `POST /health`, `POST /armut-response`, `POST /armut-exchange`
- New DTOs: `DeclareHealthRequest`, `AcceptArmutRequest`, `ExchangeArmutCardsRequest`
- `DtoMapper.cs` — handle new phases and new view fields

**Frontend**:
- `App.tsx` — handle new phases, show `ReservationDialog` for health check (Gesund/Vorbehalt buttons) and check phases, new `ArmutPartnerDialog` and `ArmutCardExchangeDialog`
- `ReservationDialog.tsx` — health-check mode (Gesund/Vorbehalt buttons vs. current reservation list)
- New `ArmutPartnerDialog.tsx`
- New `ArmutCardExchangeDialog.tsx`
- `api/game.ts` — add `declareHealth`, `respondToArmut`, `exchangeArmutCards`
- `types/api.ts` — new request/response types

### Key Design Decisions

1. **Single vs. multi Vorbehalt in SoloCheck**: tracked via `HealthDeclarations` — if exactly 1 player said Vorbehalt, they get offered all eligible reservations (not just Solos)
2. **Armut partner**: not known at declaration time; `ArmutPlayer` is set when Armut wins, `ArmutRichPlayer` when a partner accepts
3. **Card exchange**: automatic trump transfer when entering `ArmutCardExchange`; rich player then selects cards to return
4. **Schwarze Sau**: game enters Playing with `SchwarzesSau = true` flag; mid-game Solo interruption deferred to a future todo
5. **Schlanker Martin by elimination**: last Vorbehalt player in HochzeitCheck with no Hochzeit is forced to declare Schlanker Martin
