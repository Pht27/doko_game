Wenn man Neues Spiel im Resultscreen drückt kommt man zurück in die Lobby Ansicht- asntelle dessen sollte ein neues Spiel gestartet werden, wo auch direkt alle Spieler reingeladen werden. (Lass uns sagen alle 4 Spieler müssen confirmen, also jeder den Knopf drücken mit Anzeige wie viele schon gedrückt haben und zurückziehen durch nochmal drücken)

Dann sollte man evtl auch die Rundenlogik aus dem richtigen Doppelkopf übernehmen:
Die Person die rauskommt rotiert in Spielrichtung (also gegen den Uhrzeigersinn). Außer wenn Spielmodi / Vorbehalte gespielt wurden, die den Rauskommer bestimmen:
- Armut
- Jegliches Solo (including Schlanker Martin)
- Schmeißen offensichtlicherweise (Hier muss auch noch ein Resultscreen kommen mit einem sehr vereinfachten Display für Geschmissen, da es ja keine Punkte oder Augen gab)

Da fällt mir auf: Bei einem Solo muss noch die Person die rauskommt (ie erste Karte spielt) geändert werden zu der die das SOlo angemeldet hat.

Wichtig ist hier die Unterscheidung zwischen rauskommen mit VOrbehalten und rauskommen mit erster Karte -> meistens die gleiche Person außer bei Armut und Soli. Das rotierende ist das natürlich das rauskommen mit VOrbehalt.


Da wir jetzt schon im größeren Sinne Lobbylogik angehen, würde ich auch gerne für die 4 Spieler einer Lobby die gesammlten Punkte speichern (wir bleiben hier noch bei in Memory, um eine persistierte DB kümmern wir uns später).

Dazu muss glaube ich (gib hier gerne Feedback) der Datenfluss geändert werden: Evtl. lenken wir ein GameResultDTO in erstmal eine Instanz, die dann die Nettopunkte für jeden Spieler berechnet (oder ist die Instanz unnötig?), die leitet dann weiter an eine Art Lobby Verwaltung, der die Ergebnisse auf den Punktestand in der Lobby anwendet. Dann kann diese individuelle Ergebnis (Nettopunkte) samt Zusammensetzung an das Frontend geschickt werden, wo der Resultscreen anzeigt, wie viele Nettopunkte man für dieses SPiel bekommen hat, aber auch wie die Standings sind in der Lobby.

Zu guter letzt sollen dann die Lobbystandings auch noch einsehbar sein durch ein Overlay während des Spiels: Wenn man auf die GameInfo anklickt, kommt dann so ein Overlay, das nochmal die GameInfo displayed aber auch die LobbyStandings.

---

## Implementierungsplan

### Solo-Punkte-Formel
- Solo-Spieler: ±TotalScore (Extrapunkte × soloFactor bereits enthalten)
- Jeder Gegner: ∓TotalScore / soloFactor
- Nullsumme: TotalScore − 3 × (TotalScore/3) = 0 ✓
- Hinweis: GameScorer muss ggf. angepasst werden, damit Extrapunkte auch mit soloFactor multipliziert werden

### Rauskommer nach Schmeißen
- Bleibt gleich (kein echtes Spiel → kein Vorschub)

---

### Phase 1: Lobby-Standings + New-Game-Confirmation

**Backend:**
1. `LobbyState`: `int[] Standings` (by seat), `HashSet<PlayerId> NewGameVotes`, Methoden `AddNewGameVote(PlayerId) → bool allReady`, `RemoveNewGameVote(PlayerId)`, `UpdateStandings(int[] delta)`, `ResetNewGameVotes()`
2. Neue Endpunkte:
   - `POST /lobbies/{lobbyId}/new-game/ready` — Vote hinzufügen; wenn alle 4 → neues Spiel auto-starten + `gameStarted` event
   - `POST /lobbies/{lobbyId}/new-game/withdraw` — Vote zurückziehen + `newGameVoteChanged { count }` event
3. Neues `NetPointsCalculator.Calculate(GameResult, GameState) → int[]` (by seat):
   - Normal (soloFactor=1): winner seats +TotalScore, loser seats −TotalScore
   - Solo (soloFactor>1): solo seat ±TotalScore, je Gegner ∓TotalScore/soloFactor
4. `GamesController.PlayCard`: nach `gameFinished` → Lobby per GameId suchen, Nettopunkte berechnen, Standings updaten, in `gameFinished`-Event einbetten
5. `GameResultDto`: `NetPointsPerSeat: int[]`, `LobbyStandings: int[]` hinzufügen
6. `ILobbyRepository`: Methode `GetByGameId(GameId)` hinzufügen

**Frontend:**
1. `ResultScreen`: Nettopunkte je Spieler + Standings-Tabelle anzeigen
2. "Neues Spiel"-Button → sendet `/new-game/ready`, zeigt `X/4 bereit`, nochmal klicken = withdraw
3. Neue API-Funktionen: `voteNewGame(lobbyId, token)`, `withdrawNewGame(lobbyId, token)`
4. `useLobby` / `useGameState`: `newGameVoteChanged`-Event empfangen → Zähler aktualisieren

---

### Phase 2: Rauskommer-Rotation

**Backend:**
1. `LobbyState`: `int VorbehaltRauskommer` (Sitz-Index, Start=0), `AdvanceRauskommer()` (+1 mod 4)
2. `GameState`: `PlayerId VorbehaltRauskommer` (gesetzt beim Spielstart)
3. `DealCardsHandler`: ersten Zug auf `VorbehaltRauskommer` statt `Players[0]` setzen
4. `MakeReservationHandler`: SpieleRauskommer bestimmen:
   - Normal / Hochzeit → `state.VorbehaltRauskommer`
   - Solo → Solo-Anmelder
   - Armut → `state.ArmutPlayer`
5. New-Game-Auto-Start (Phase 1): `lobby.AdvanceRauskommer()` vor Spielstart aufrufen (außer nach Schmeißen)

---

### Phase 3: GameInfo-Overlay + Standings während des Spiels

**Backend:**
1. `PlayerGameViewDto`: `LobbyStandings: int[]` hinzufügen (aus Lobby beim Spielstart)

**Frontend:**
1. `GameInfo`: anklickbar machen → Overlay öffnen
2. Neues `LobbyStandingsOverlay`: zeigt Spielinfo + Standings

---

### Phase 4: Schmeißen-Result-Screen

**Backend:**
1. Bei `GamePhase.Geschmissen`: `gameGeschmissen { schmeisser: int }` SignalR-Event senden
2. Keine Punkte, Votes resetten, `newGameVoteChanged` senden

**Frontend:**
1. Neues `GeschmissenResultScreen`: "Geschmissen!"-Anzeige mit gleicher Bereit-Confirmation
2. `useGameState`: `gameGeschmissen`-Event empfangen

---

### Kritische Dateien

| Datei | Phase |
|-------|-------|
| `Doko.Domain/Lobby/LobbyState.cs` | 1, 2 |
| `Doko.Api/Controllers/LobbiesController.cs` | 1, 2 |
| `Doko.Api/Controllers/GamesController.cs` | 1 |
| `Doko.Api/DTOs/Responses/GameResultDto.cs` | 1 |
| `Doko.Application/Games/Handlers/DealCardsHandler.cs` | 2 |
| `Doko.Application/Games/Handlers/MakeReservationHandler.cs` | 2 |
| `Doko.Domain/GameFlow/GameState.cs` | 2 |
| New: `Doko.Domain/Scoring/NetPointsCalculator.cs` | 1 |
| `src/components/ResultScreen/ResultScreen.tsx` | 1 |
| `src/api/lobby.ts` | 1 |
| `src/hooks/useGameState.ts` | 1, 4 |
| `src/components/shared/GameInfo.tsx` | 3 |
| New: `src/components/ResultScreen/GeschmissenResultScreen.tsx` | 4 |
| New: `src/components/shared/LobbyStandingsOverlay.tsx` | 3 |