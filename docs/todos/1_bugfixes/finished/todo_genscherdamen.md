im results screen fällt auf, dass das genschern noch nicht ganz funktioniert -> der genscherer und sein neuer partner müssen jetzt re sein. die ganze logik wo die füchse liegen und so muss dann auch auf die neuen parteien angepasst werden!

bitte prüfe den aktuellen stand auf funktionalität. hier ist nochmal genau beschrieben, wie es sein sollte:

# Genscherdamen / Gegengenscherdamen — Open TODOs

## What already works
- Eligibility detection: `GenscherdamenSonderkarte.AreConditionsMet` and `GegengenscherdamenSonderkarte.AreConditionsMet` are correct.
- Sonderkarte activation itself (the `ActivateSonderkarteModification`) is applied by `PlayCardUseCase`.

## What is still missing

### 1. Partner selection
When a player activates Genscherdamen/Gegengenscherdamen they must choose a new partner.
This requires:
- `PlayCardCommand` extended with `PlayerId? GenscherPartner` (passed only when Genscherdamen/Gegengenscherdamen is activated).
- `ConsoleInputReader.PromptCard` (and `PromptSonderkarten`) must prompt for partner selection immediately after the player confirms activation.
- `PlayCardUseCase` must apply a `SetPartyResolverModification(new GenscherPartyResolver(...))` when the partner has been chosen.
- `GenscherPartyResolver` needs to be created: stores (genscherPlayer, chosenPartner) → they form Re; the other two form Kontra.
- The player may choose any of the other three players, including their current partner.

### 2. "Nicht tauschen" — keeping original teams but switching Re/Kontra labels
If the Genscher picks their **current partner**, teams do NOT change.
The Genscher's team becomes Re (even if they were previously Kontra), and the other team becomes Kontra.
The GenscherPartyResolver handles this the same way as a real swap — the label assignment just happens to match the original composition.

### 3. Announcements after Genschern
No announcements are allowed after Genscherdamen or Gegengenscherdamen fires.
Previously made announcements:
- If teams DID change: all prior announcements are invalid and ignored when scoring. Feigheit rules do not apply.
- If teams did NOT change (Nicht tauschen): prior announcements stay intact and move with their party labels.

### 4. Fuchs / Gans / Klabautermann — re-evaluation after party change
After a Genscher fires and the party assignments change, all Extrapunkte that depend on party membership must be re-evaluated at scoring time against the **new** parties.

Example: Players 1+2 are Re, 3+4 are Kontra. Player 2's fox lands with Player 1 during play — no Extrapunkt (same team). Then Player 3 genschers to Player 1. Now 1+3 are Re and 2+4 are Kontra. At the end, Player 1 holds Player 2's fox, but Player 2 is now Kontra → **Fuchs gefangen** Extrapunkt must trigger.

The same logic applies to Gans and Klabautermann.

### 5. Gegengenscherdamen partner selection
Same flow as Genscherdamen. The Gegengenscher also picks a new partner.
The new partnership overrides the Genscherdamen partnership.
If the new partnership happens to reconstruct the pre-Genscherdamen teams, treat it as a full restoration (including prior announcements).
