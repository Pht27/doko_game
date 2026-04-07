Bei der Armut stehen immer noch direkt die 3 Optionen P0 P1 und P2 beim Vorbehlat anmelden da (Frontend Problem). Außerdem soll man die Karten die man zurückgeben will aus seiner Hand auswählen können und nicht in dem komischen Dialog.

Außerdem muss bei der Armut noch richtig bestimmt werden, wer rauskommt. (Die Person in der Reihenfolge vor der Reichen partei)

Und es muss richtig angesagt werden, wie viele Karten zurück und ob Trump dabei war. (nur Frontend?)

---

## Implementation Plan

### Problems

1. **Reservation dialog shows P0/P1/P2 for Armut** — `ReservationDialog.tsx` has a special branch
   that renders partner-selection buttons. In the phased system the partner is not chosen at
   declaration time; Armut should just be a single button.

2. **Card exchange uses a center dialog** — `ArmutCardExchangeDialog` shows cards as text buttons.
   The user wants to select cards directly from the graphical hand display at the bottom.

3. **Wrong first player after exchange** — `ExchangeArmutCardsUseCase` always starts with
   `state.Players[0]`. Correct: the player sitting immediately before the rich player in seat order
   leads the first trick.

4. **Exchange announcement not shown** — after the rich player returns cards, all players should
   see "X Karten zurückgegeben [mit/ohne Trump]". The event data exists in
   `ArmutCardsExchangedEvent`; it just isn't surfaced to the UI.

### Files Affected

**Backend**
- `GameStateModification.cs` — add `SetArmutReturnedTrumpModification`
- `GameState.cs` — add `ArmutReturnedTrump (bool?)` property and handle new modification
- `ExchangeArmutCardsUseCase.cs` — fix first-player logic; apply new modification
- `DtoMapper.cs` — `BuildArmut` no longer needs `ArmutPartner` from request (use `player` as placeholder rich player)
- `SignalRGameEventPublisher.cs` — add handler for `ArmutCardsExchangedEvent`
- `PlayerGameView.cs` — add `ArmutExchangeCardCount (int?)` and `ArmutReturnedTrump (bool?)` init properties
- `GameQueryService.cs` — populate new view fields
- `PlayerGameViewResponse.cs` — add corresponding DTO fields
- `DtoMapper.cs` — map new fields in `ToResponse(PlayerGameView)`

**Frontend**
- `types/api.ts` — add `armutExchangeCardCount`, `armutReturnedTrump` to `PlayerGameViewResponse`; remove Armut-specific partner handling
- `ReservationDialog.tsx` — remove the Armut-specific branch; treat Armut as a plain button
- `HandDisplay.tsx` — add optional selection-mode props (`selectionMode`, `selectedCardIds`, `onCardSelect`, `maxSelection`)
- `App.tsx` — replace center `ArmutCardExchangeDialog` with info panel; route hand-clicks to selection when in exchange mode; show exchange announcement in GameInfo area
- `ArmutCardExchangeDialog.tsx` — delete (replaced by inline hand selection)
