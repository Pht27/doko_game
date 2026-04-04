# Missing Implementations

Domain layer pieces that are still stubbed out (`throw NotImplementedException`).
These need to be implemented before the game engine can run end-to-end.

---

## 1. `GameState.Apply(GameStateModification)`

The only place state mutations occur. Needs to handle all modification types:

- `ReverseDirectionModification` — flip `Direction` (Counterclockwise ↔ Clockwise)
- `WithdrawAnnouncementModification` — remove an announcement from `Announcements`
- `TransferCardPointsModification` — record a card-point transfer (Schatz)

Additionally, when a sonderkarte activates, the engine must:
- Add it to `ActiveSonderkarten`
- Rebuild `TrumpEvaluator` with the appropriate `ISonderkarteRankingModifier`s
  (Schweinchen/Superschweinchen/Hyperschweinchen/Heidmann/Heidfrau)

Festmahl and Blutbad awards have `Delta = 0`; the engine must check for them and
use `BenefittingPlayer` as the authoritative trick winner override.

---

## 2. `GameState.NextPlayer(PlayerId, PlayDirection)`

Rotates the turn to the next player seat in the given direction.

---

## 3. `Hand.Remove(Card)`

Returns a new `Hand` without the specified card. Throws if card is not present.

---

## 4. `Deck.Standard48()` / `Deck.Standard40()`

Creates the full 48-card deck (two copies of each of 24 distinct cards).
`Standard40()` omits the Nines.

---

## 5. Card Play Validity — "Bedienen" (Follow-Suit Rule)

A player must follow the led suit if able. Needs a domain service or method:

```
bool CanPlay(Card card, Hand hand, Trick currentTrick, ITrumpEvaluator trumpEvaluator)
```

Rules:
- If the led card is **trump**: player must play trump if they have any.
- If the led card is a **plain suit**: player must play that same plain suit if they have any.
- If unable to follow suit: any card may be played.
- A player holding **only one** of the required suit may still play a different card if they
  have no other option — the check is purely "does the hand contain any card of the required
  category".

Edge cases:
- ♥ 10 (Dulle) counts as trump, not as plain Herz.
- Sonderkarte effects on trump status (Schweinchen upgrading ♦A, etc.) are already
  reflected in the `ITrumpEvaluator` passed in — no special-casing needed here.

---

## 6. `AnnouncementRules`

Three static methods, all stubs:

- `CanAnnounce(PlayerId, AnnouncementType, GameState)` — timing window, consecutive
  ordering (Re/Kontra before Keine90, etc.), party membership.
- `IsMandatory(PlayerId, GameState)` — Pflichtansage: first trick ≥ 35 Augen.
- `ViolatesFeigheit(GameResult, GameState)` — winning party missing > 2 announcements
  relative to the margin.

---

## 7. `IGameScorer`

Calculates `GameResult` from a `CompletedGame`:

- Sum Augen per party.
- Apply win/loss threshold (Re needs 121+).
- Add base game value components (Gewonnen, Gegen die Alten, Keine90/60/30, Schwarz).
- Add/subtract Extrapunkte (offset Re vs. Kontra awards).
- Check Feigheit and adjust if applicable.
- Triple solo player's score for Soli.
