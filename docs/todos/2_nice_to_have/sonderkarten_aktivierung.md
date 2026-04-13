Das muss irgendwie schöner gehen, diese Dialoge sind ugly as hell. Momentan gibt es ja keine doppelt besetzten Sonderkarten, also kann man auch einfach einen Dialog poppen mit "<Sonderkarte> aktivieren?". Beim Genschern sollten dann auch zwei Dialoge kommen, oder der "neuen Partner auswählen" Teil soll erst auftauchen, wenn man genschern aktiviert hat.

Außerdem soll beim Genschern es gar nicht erst möglich sein, sich selbst auszuwählen (im Frontend), um den Fehler zu vermeiden.

## Plan

Replace the checkbox-form overlay with a two-step, mobile-friendly confirm dialog:

**Step 1 — Confirm each sonderkarte** (one at a time):
- Show name + description
- Two big tap targets: "Aktivieren" / "Überspringen" (play card without activating)
- Small "Abbrechen" link at the bottom to cancel the card play entirely

**Step 2 — Genscher partner (only when a Genscher-type was activated)**:
- Show a list of player buttons (tap-friendly), excluding the active player
- "Bestätigen" button

**Files affected:**
- `SonderkarteOverlay.tsx` — full redesign with step state
- `SonderkarteOverlay.css` — new styles (simpler, mobile-first)
- `GameOverlays.tsx` — thread `activePlayer` through
- `GameBoard.tsx` — pass `activePlayer` to `GameOverlays`
- `translations.ts` — add `aktivieren`, `ohneAktivieren`, `genscherPartnerWaehlen`

**Trade-offs:**
- Since there's currently always exactly one eligible sonderkarte per card, the
  sequential logic is effectively single-step. When multiple exist in the future,
  the component already handles them correctly one-by-one.
- Self-selection for Genscher partner is prevented by filtering `activePlayer` out
  of the partner list.