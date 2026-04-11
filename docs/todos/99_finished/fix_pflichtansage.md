Pflichtansagen gehen nicht (oder werden nicht angezeigt, aber ich vermute ersteres). Bitte reviewe nochmal die Regeln für eine Pflichtansage und dann checke, ob sie im Backend richtig umgesetzt ist.

Meine Recherche ergibt folgendes:

1. In AnnouncementRules ist IsMandatory nicht ganz sinnvoll. Der Workflow muss so sein: Ein Stich wird beendet und dann muss (von der SPiel Engine) entschieden werden, ob aus diesem Stich für die STichgewinner eine Pflichtansage entsteht. Es müsste also eine Funktion geben, die einen Trick entgegen nimmt und einen boolean zurückgibt, ob daraus eine Pflichtansage folgt. (Die Regel ist, dass im ersten Stich >= 35 AUgen sein müssen, damit eine Pflichtansage entsteht. Im zweiten entsteht für den Gewinner nur eine Pflichtansage, wenn im ersten und zweiten Stich >= 35 Augen waren. Danach gibt es keine Pflichtansagen mehr. Pflichtansagen gibt es im Normalspiel, in Armut und in stiller Hochzeit sowie Kontrasolo)
2. Im PlayCardHandler wird in CompleteTrickAsync nichts mit Pflichtansagen gemacht. Dort müsste bei einem CompleteTrick die oben beschreibene Funktion aufgerufen werden. Falls true

---

## Implementation Plan

### Root Cause

Two bugs found:

**Bug 1 — Missing first-trick check in `IsMandatory`** ([AnnouncementRules.cs:99-114](src/backend/Doko.Domain/Announcements/AnnouncementRules.cs#L99-L114)):
The second-trick Pflichtansage is triggered when `secondTrick.Points >= 35`, but the code does **not** also check that `firstTrick.Points >= 35`. The rule requires **both** tricks to have >= 35 Augen.

**Bug 2 — `CompleteTrickAsync` never enforces Pflichtansagen** ([PlayCardHandler.cs:205-234](src/backend/Doko.Application/Games/Handlers/PlayCardHandler.cs#L205-L234)):
After a trick completes, there is no call to `IsMandatory` or anything that auto-makes the forced announcement. The `IsMandatory` method exists but is never called from the game flow.

### What changes

**`AnnouncementRules.cs`:**
1. Fix bug in `IsMandatory`: add `firstTrick.Points >= 35` check for the second-trick condition.
2. Add new method `GetMandatoryAnnouncement(PlayerId winner, GameState state) → AnnouncementType?`:
   - Returns the announcement type the winner must make after the latest completed trick, or null.
   - Called after `AddCompletedTrickModification` has been applied (trick already in `CompletedTricks`).
   - Logic mirrors `IsMandatory` but returns the specific type instead of a bool.

**`PlayCardHandler.cs`:**
- In `CompleteTrickAsync`, after `state.Apply(new AddCompletedTrickModification(...))`:
  - Call `AnnouncementRules.GetMandatoryAnnouncement(winner, state)`
  - If non-null: apply `AddAnnouncementModification` and emit `AnnouncementMadeEvent`
  - This auto-makes the Pflichtansage on behalf of the player (no player input needed — it's forced)

**`AnnouncementRulesTests.cs`:**
- Add test for the bug: second-trick Pflichtansage should NOT trigger if first trick had < 35 Augen
- Add tests for `GetMandatoryAnnouncement`

**`PlayCardHandlerTests.cs`:**
- Add integration test: complete a high-value first trick → announcement is auto-emitted
