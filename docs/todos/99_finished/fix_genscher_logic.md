Ich mag nicht, wie Genschern in der Application / Domain gehandlet wird:

Der PlayCardCommand kommt mit einem potentiellen Genscherpartner, das ist okay. Aber dass der game State im PlayCardHandler modifiziert wird ist falsch: Der GameState wird nur von sich selbst durch Apply von GameStateModifications verändert - bei der Code Recherche hatte ich keine Chance in der Domain der Genscher Effekt zu finden, weil er nicht auftaucht.

Da muss man nochmal darüber nachdenken, wie man das handlet. Problematisch ist schon, dass die Apply Methode keinen extra parameter nimmt. Das könnte man vielleicht ändern, indem man da irgendwie Parameter einfügt, allerdings wäre es auch doof wenn da jede Sonderkarte eine GenscherID in den Parametern stehen hätte... Kann man irgendwie über generische Parameter gehenm also sowas wie Parameter<T> wobei T eine Sonderkarte ist?

Da muss man sich nochmal den Kopf zerbrechen aber so wie es ist kann es nicht bleiben

---

## Implementation Plan

### Probleme

**Problem 1: Direkte Trick-Mutation**
`PlayCardHandler.PlayCardIntoTrick` ruft `state.CurrentTrick!.Add(new TrickCard(card, player))` direkt auf –
das umgeht das Apply-Muster komplett.

**Problem 2: Genscher-Effekt im Domain nicht sichtbar**
`ApplyGenscherIfNeeded` im Handler wendet `SetPartyResolverModification(new GenscherPartyResolver(...))` an.
Das ist zwar technisch Apply, aber:
- Der Modifikationsname sagt nicht „Genscher"
- Die `GenscherPartyResolver`-Instanziierung passiert in der Application-Schicht, nicht in der Domain
- Beim Lesen des Domain-Codes findet man die Genscher-Wirkung nicht

### Lösung

**1. `AddCardToTrickModification(PlayerId Player, Card Card)` in `GameStateModification.cs`**
- Handler ruft `state.Apply(new AddCardToTrickModification(player, card))` statt direktem `.Add()`
- `GameState.Apply()` kapselt `CurrentTrick!.Add(new TrickCard(m.Card, m.Player))`

**2. `SetPartyResolverModification` durch `SetGenscherPartnerModification(PlayerId Genscher, PlayerId Partner)` ersetzen**
- Semantisch klar: dieser Modification-Typ sagt explizit „Genscher wählt Partner"
- `GameState.Apply()` erstellt intern `new GenscherPartyResolver(m.Genscher, m.Partner)` → Domain-Code ist der richtige Ort dafür
- Handler übergibt nur `command.Player` + `command.GenscherPartner` – keine Kenntnis von `GenscherPartyResolver` mehr nötig
- `SetPartyResolverModification` wird gelöscht (war ausschließlich für Genscher genutzt)

### Betroffene Dateien
- `Doko.Domain/Sonderkarten/GameStateModification.cs` — zwei neue Records, `SetPartyResolverModification` entfernt
- `Doko.Domain/GameFlow/GameState.cs` — zwei neue Apply-Cases, alter Case entfernt
- `Doko.Application/Games/Handlers/PlayCardHandler.cs` — beide Stellen aktualisiert
- `Doko.Domain/Sonderkarten/GenscherdamenSonderkarte.cs` — Kommentar aktualisiert
- `Doko.Domain/Sonderkarten/GegengenscherdamenSonderkarte.cs` — Kommentar aktualisiert

### Nicht gelöst (bewusst)
Das `ISonderkarte.Apply(GameState)` kennt weiterhin keinen Partner-Parameter –
der Partner ist User-Input und kommt über den Command. Der Handler übersetzt ihn in eine
semantische Domain-Modification (`SetGenscherPartnerModification`), was den Bruch sauber hält.