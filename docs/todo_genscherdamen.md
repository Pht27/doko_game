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

### 2. "Nicht tauschen" — keeping original teams but switching Re/Kontra labels
If the Genscher does NOT swap teams (keeps the current partner), the original Re party stays intact,
but the Genscher's team is now labelled Re (even if they were previously Kontra).
The `GenscherPartyResolver` must handle this case.

### 3. Announcements — party label changes
Announcements are tied to parties (Re/Kontra). When Genscherdamen fires:
- If teams DID change: existing Re announcements may need to be re-assigned to the new Re party.
- If teams did NOT change but Re/Kontra labels switched: same re-assignment applies.
- If Gegengenscherdamen subsequently fires and restores the original teams, the announcements
  should be restored to their original party labels.

This is complex; defer announcement handling until party resolver logic is solid.

### 4. Gegengenscherdamen partner selection
Same flow as Genscherdamen. The Gegengensch er also picks a new partner.
The new partnership overrides the Genscherdamen partnership.
Original-team restoration: if the new partnership happens to reconstruct the pre-Genscherdamen
teams, treat it as a full restoration (including announcement labels).

### 5. Edge case: Gegengenscherdamen restoring original teams
Track the "pre-Genscherdamen" party assignment so it can be fully restored if Gegengenscherdamen
picks the same pairing. Simplest approach: store the original `IPartyResolver` in `GameState`
before the first Genscher fires.
