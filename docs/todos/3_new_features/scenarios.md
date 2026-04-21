# Szenario Feature

Ich würde gerne das Szenario feature aus der ConsoleApp einführen

in der lobby details view sollte eine option "Szenario laden" sein, wo man dann eines der Szenarios auswählt und wenn dann das Spiel startet, werden die Karten so verteilt.

Wir sollten auch nochmal checken ob die Szenarios noch aktuell sind / ob man evtl sogar welche hinzufügen kann. (Kontrasolo!)

Dann können wir die Console App eigentlich auch löschen, da sie keinen Zweck mehr erfüllt.

---

## Implementation Plan

### Overview

Move the scenario feature from the ConsoleApp into the full backend/frontend stack so that lobbies can load predefined card distributions.

### What changes

**1. Move scenario code to Application layer**
- New: `Doko.Application/Scenarios/ScenarioConfig.cs` — the config class (same as ConsoleApp)
- New: `Doko.Application/Scenarios/Scenarios.cs` — all scenario definitions + `All` list (same as ConsoleApp, plus Kontrasolo)
- New: `Doko.Application/Scenarios/ScenarioShuffler.cs` — the shuffler (moved from ConsoleApp, implements `IDeckShuffler` from same layer, no violation)

**2. Add Kontrasolo scenario**
- Player holds both ♠ Queens (Damen) + both ♠ Kings (Könige) → triggers `KontraSolo` silent game mode

**3. LobbyState gets a selected scenario**
- Add `string? SelectedScenario` property
- Add `SetScenario(string? name)` method

**4. DealCardsCommand/Handler use scenario**
- `DealCardsCommand` gets `string? ScenarioName = null`
- `DealCardsHandler` resolves shuffler: if `ScenarioName` is set → `new ScenarioShuffler(config)`, else use injected `IDeckShuffler`

**5. Two new API endpoints on LobbiesController**
- `GET /lobbies/scenarios` — returns list of all scenario names (unauthenticated)
- `POST /lobbies/{lobbyId}/scenario` — sets/clears scenario on lobby, body: `{ name: string | null }` (requires auth + must be seated)
- Broadcasts `scenarioChanged` SignalR event
- `LobbyViewResponse` gets `string? SelectedScenario` field
- When starting game (`VoteLobbyReady` + `VoteNewGame`): pass `lobby.SelectedScenario` in `DealCardsCommand`

**6. Frontend**
- `lobby.ts`: add `getScenarios()` and `setScenario(token, lobbyId, name)` API calls
- `useLobby` hook: add `selectedScenario` state, update on initial fetch + `scenarioChanged` SignalR event
- `LobbyDetailView`: add a "Szenario" section below the invite link — shows current scenario name + "Laden" button that opens a modal with the list. Only visible when user is seated. "Kein Szenario" clears it.

**7. Delete ConsoleApp**
- Remove `Code/backend/Doko.Console/` project entirely
- Remove from solution file

### Files affected

| File | Action |
|------|--------|
| `Doko.Application/Scenarios/ScenarioConfig.cs` | New |
| `Doko.Application/Scenarios/Scenarios.cs` | New |
| `Doko.Application/Scenarios/ScenarioShuffler.cs` | New |
| `Doko.Domain/Lobby/LobbyState.cs` | Add SelectedScenario |
| `Doko.Application/Games/Commands/DealCardsCommand.cs` | Add ScenarioName param |
| `Doko.Application/Games/Handlers/DealCardsHandler.cs` | Pick shuffler from scenario |
| `Doko.Api/DTOs/Responses/LobbyResponses.cs` | Add SelectedScenario to LobbyViewResponse |
| `Doko.Api/Controllers/LobbiesController.cs` | New endpoints + pass scenario on start |
| `Code/backend/Doko.Console/` | Delete |
| `frontend/src/api/lobby.ts` | Add getScenarios, setScenario |
| `frontend/src/hooks/useLobby.ts` | Add selectedScenario |
| `frontend/src/components/MultiplayerBrowserPage/LobbyDetailView.tsx` | Add scenario UI |
