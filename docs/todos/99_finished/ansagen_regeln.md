wir müssen die ansageregeln etwas anpassen:

eigentlich unterscheidet man zwischen "Ansage" und "Absage". Ich erkläre das und dann kann man überlegen, ob es sich lohnt die technisch zu unterscheiden, da sie doch sehr ähnlich sind. "Re" und "Kontra" sind Ansagen, alles darüber hinaus (keine 90, keine 60, keine 30, schwarz) sind Absagen.

Beim Nicht-Erfüllen eine Absage verliert man automatisch das Spiel (zB Re sagt keine 90 an und Kontra kriegt 90+ Punkte)

Die Ansagen sind nicht gleichzusetzen mit einer Absage "keine 120", Ansagen werden erfüllt, je nachdem ob das Spiel gewonnen wird. Beispiele in denen dieser Unterschied relevant ist:
- Kontra sagt "Kontra" an und scored dann 120 Augen. Damit hat Re auch 120 Augen. Trotzdem hat Kontra seine Ansage erfüllt und kriegt den Punkt, da Kontra nach Definition das Spiel gewonnen hat (obwohl Re 120 Augen hat!)
- Re sagt "Re" und "keine 90" an und Kontra sagt "Kontra" an. Am Ende hat Kontra 97 Punkte. Dann gewinnt Kontra das SPiel (weil Re seine Absage nicht erreicht hat) UND kriegt den Punkt für "Kontra", da sie das Spiel gewonnen haben. (Außerdem kriegen sie die Punkte für die "Re" und "Re", "keine 90" Ansagen)

Dabei tritt ein Edge Case auf, der aber durch Zusatzregeln verhindert wird:
1. nur eine der beiden parteien kann eine keine 90 oder höhere Absage machen. Wenn also sowohl "Re" als auch "Kontra" angesagt wurden und dann Re "keine 90" ansagt, kann Kontra keine Absagen machen. Re kein weiter keine 60 usw. absagen

Die Logik ist also wie folgt.
- jede An- und Absage erhöht den Spielwert um 1
- falls eine Partei Absagen gemacht hat, gewinnt sie das Spiel nur wenn sie alle diese erfüllt hat
- falls niemand Absagen gemacht hat, wird das Spiel durch die 120 Augen Logik entschieden (Re braucht 121, Kontra 120 zum gewinnen)
- der Gewinner eines Spiels kriegt dann die Punkte = (Spielwert + Extrapunkte) * soloFactor und der Verlierer verliert diese Punkte

Bitte checke inwiefern diese Logik bei uns umgesetzt ist und passe das sonst an.

---

## Implementierungsplan

### Befund

Zwei Dinge fehlen:

**Bug 1: Gewinnbestimmung ignoriert nicht erfüllte Absagen** (`GameScorer.cs:46`)

Aktuell:
```csharp
Party winner = reAugen >= 121 ? Party.Re : Party.Kontra;
```
Das ist falsch. Wenn Re "keine 90" ansagt und Kontra 97 Punkte holt (≥90), verliert Re automatisch – unabhängig von den Augen.

**Bug 2: `CanAnnounce` sperrt Absagen nicht, wenn Gegenseite schon eine Absage gemacht hat** (`AnnouncementRules.cs:43`)

Der Mutex fehlt: Sobald eine Partei eine Keine90+ Absage macht, darf die andere Partei keine Absagen mehr machen.

---

### Änderungen

**`Doko.Domain/Scoring/GameScorer.cs`** – Schritt 2 "Determine winner":

Vor der Augen-Logik prüfen, ob eine Partei nicht erfüllte Absagen hat:
- Re hat Keine90 angesagt → Kontra muss <90 Augen haben, sonst verliert Re automatisch
- Gleiches für Keine60, Keine30, Schwarz
- Gilt symmetrisch für Kontra
- Nur wenn keine Absage verletzt wurde → normale Augen-Logik (Re braucht 121)

**`Doko.Domain/Announcements/AnnouncementRules.cs`** – `CanAnnounce`:

Für alle Absagen (Keine90/60/30/Schwarz): zusätzlich prüfen, ob die Gegenseite bereits eine Absage gemacht hat. Falls ja → nicht erlaubt.

**Neue Tests**:
- `GameScorerTests`: Re sagt "keine 90" an, Kontra holt 97 → Kontra gewinnt
- `GameScorerTests`: Kontra sagt "keine 90" an, Re holt 95 → Re gewinnt
- `AnnouncementRulesTests`: Keine90 gesperrt wenn Gegenseite schon Keine90 hat
