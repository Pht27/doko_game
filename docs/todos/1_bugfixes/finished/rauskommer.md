momentan rotiert in einer lobby richtigerweise der healthcheck rauskommer aber nicht der karten rauskommer (der die erste karte legt).

eigentlich ist es ja fast immer so, dass der karten rauskommer der vorbehalt (healtcheck) rauskommer ist (außer bei solos und armut).

vielleicht können wir das ja irgendwie koppeln, dass beim spiel anfang der karten rauskommer auch auf den vorbehalt rauskommer gesetzt wird, dann muss nur drauf geachtet werden, dass bei den sonderfällen das halt während des health checks überschrieben wird.

## Plan

### Root Cause

In [DeclareHealthStatusHandler.cs:88](Code/backend/Doko.Application/Games/Handlers/DeclareHealthStatusHandler.cs#L88), when no player declared a Vorbehalt (normal game), the code sets CurrentTurn to `state.Players[0].Id` — always the same player regardless of who the VorbehaltRauskommer is.

```csharp
// BUG: should be state.VorbehaltRauskommer
state.Apply(new SetCurrentTurnModification(state.Players[0].Id));
```

### Fix

**File: `DeclareHealthStatusHandler.cs`**

1. Line 88: Replace `state.Players[0].Id` with `state.VorbehaltRauskommer` — this is the correct player to play first in a normal game.

2. Lines 78–81 (`vorbehaltPlayers`): Order by seat relative to VorbehaltRauskommer (same pattern as `DealCardsHandler.cs`), so the Solo check phase also starts from the correct player:
   ```csharp
   var rauskommerSeat = (int)state.Players.First(p => p.Id == state.VorbehaltRauskommer).Seat;
   var vorbehaltPlayers = state
       .Players.Where(p => state.HealthDeclarations.TryGetValue(p.Id, out var hasV) && hasV)
       .OrderBy(p => ((int)p.Seat - rauskommerSeat + 4) % 4)
       .Select(p => p.Id)
       .ToList();
   ```

Solo and Armut already override the CurrentTurn in their own handlers (MakeReservationHandler, ExchangeArmutCardsHandler) — no changes needed there.

### Tests to add

- Normal game: VorbehaltRauskommer (not Players[0]) plays first card
- Vorbehalt present: Solo check starts with the vorbehalt player closest to VorbehaltRauskommer
