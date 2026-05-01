# Release Notes

## [1.2.0] - 2026-05-01

### Spielfläche
- Im Spiel werden jetzt die Vorbehalte und Spielphasen besser angezeigt.
- Aktive Sonderkarten werden angezeigt.
- Beim Ausspielen einer Sonderkarte wird klar angesat, was ausgespielt wurde.

### Behoben
- Nach dem letzten Stich wird jetzt die Stich-Animation vollständig abgespielt, bevor das Ergebnis erscheint.
- Opa-Spieler legen ihre Karten jetzt mit kurzer Verzögerung, sodass man sieht, wie die Karten einzeln zugespielt werden.

## [1.1.1] - 2026-04-27

### Verbessert
- Lobbies werden automatisch gelöscht, wenn sie länger als 2 Stunden inaktiv waren. So verschwinden verlassene oder abgestürzte Lobbies von selbst.

## [1.1.0] - 2026-04-26

### Neu
- Beim Start jedes Spiels erscheint kurz ein Popup, das den Spielmodus ankündigt (z. B. „Karo-Solo · S2" oder „Normalspiel"). Bei Hochzeit, Armut und Soli wird der zugehörige Spieler mit angezeigt.
- Wenn eine Sonderkarte aktiviert wird, erscheint ebenfalls ein Popup mit Name des Spielers und der Sonderkarte (z. B. „S3 · Schweinchen"). Das Popup verschwindet nach 3 Sekunden automatisch und kann auch mit einem X weggeklickt werden.

### Verbessert
- In der Lobby sehen alle Spieler, wer bereits auf „Bereit" gedrückt hat (grünes Häkchen beim Spielerplatz).

### Behoben
- Beim Schlanken Martin kommt jetzt der Spieler raus, der ihn angemeldet hat.
- Die Spielrichtung (gegen den Uhrzeigersinn) wird auf dem Spielfeld jetzt korrekt angezeigt.
- Spieler werden überall einheitlich als S1–S4 bezeichnet (statt S0–S3 im Spiel und Spieler 1–4 in der Lobby).
- In der Armut gibt es kein Karlchen, keine Agathe und kein Fischauge mehr.


## [1.0.1] - 2026-04-26

### Behoben
- App zeigt auf iOS-Geräten (iPhone, iPad, Mac Safari) jetzt korrekt Inhalte an, statt nur den lila Hintergrund.
- In der Armut-Anzeige steht jetzt „Trumpf" statt „Trump".

## [1.0.0] - 2026-04-26

### Initialer Release mit allen fertigen Doppelkopf-Regeln

## [0.2.2] - 2026-04-26

## Neu
- Auf der Regelseite verschwindet der Header nun beim scrollen

## [0.2.1] - 2026-04-26

### Behoben
- Spieler-Labels der Gegner verrutschen nicht mehr, wenn noch keine Karte gespielt wurde.
- Ansage-Abzeichen (z.B. „Keine 90") werden jetzt in der korrekten Parteifarbe angezeigt.
- Im Schlanken Martin sind Ansagen nicht mehr möglich.
- Auf Android-PWA bleibt das Querformat jetzt aktiv gesperrt; die Drehhinweis-Overlay erscheint seltener.

### Neu
- Eigenes Spieler-Label als Overlay am unteren Bildschirmrand: zeigt Name, Stichanzahl und eigene Ansage in der wahrgenommenen Parteifarbe (wie der Ansage-Button).

## [0.2.0] - 2026-04-26

### Verbessert
- Startseite: neues Layout mit dekorativen Kartensymbolen und überarbeiteten Buttons.
- Regelseite: vollständig überarbeitet mit übersichtlicherer Struktur und verbessertem Stil.
- Spielfeld: leerer Stichbereich wird nicht mehr angezeigt – bevor Karten gespielt werden, ist die Mitte leer.
- Ansage-Buttons zeigen die Parteifarbe: Re in Blau, Kontra in Lila.
- Handkarten mit aktivierbarem Sonderkarteneffekt haben einen orangen Rahmen.
- Spielernamen werden in der Parteifarbe angezeigt, sobald die Partei bekannt ist.
- Spieler-Labels zeigen die Anzahl gewonnener Stiche.
- Lobby-Browser zeigt den Erstellungszeitpunkt jeder Lobby.

## [0.1.2] - 2026-04-26

### Verbessert
- Zurück-Schaltfläche zeigt nur noch einen Pfeil (←) ohne Text und ist eine wiederverwendbare Komponente.

## [0.1.1] - 2026-04-25

### Behoben
- **Festmahl**: Bei einem Stich mit drei Tieren, wovon zwei gleich sind, gewinnt das zweite Tier der Mehrheit den Stich.
- **Blutbad**: Bei einem Stich mit drei unterschiedlichen Tieren gewinnt die nicht Tier Karte den Stich.
- **Meuterei**: Zwei Pik-Könige und eine Pik-Dame im selben Stich – der Spieler des zweiten Pik-Königs gewinnt; Klabautermann wird dabei nicht fälschlicherweise vergeben.
- Fuchs (♦A) zählt immer als Tier für Festmahl und Blutbad, unabhängig von aktiven Schweinchen.

## [0.1.0] - 2026-04-25

### Neu
- Regelseite: Alle Spielregeln übersichtlich erklärt – erreichbar über die Startseite.
- Schnellnavigation zwischen den Abschnitten (Karten & Trumpf, Parteien, Soli, Sonderkarten, …) ohne Scrollen.

### Behoben
- Auf iOS erscheint der grüne „Bitte drehen"-Screen nicht mehr mitten im Spiel – die Ausrichtung ist systemseitig gesperrt.

## [0.0.2] - 2026-04-25

### Verbessert
- Die Startseite wird jetzt im Hochformat angezeigt – kein Drehen nötig, bevor man ins Spiel geht.
- Lobby und Spiel wechseln automatisch ins Querformat, sobald man die Startseite verlässt.
- PWA-Splash-Screen erzwingt kein Querformat mehr – kein ruckartiges Drehen beim App-Start.

## [0.0.1] - 2026-04-25

### Neu
- Mehrspieler-Doppelkopf für 4 Spieler
- Lobby-System mit Einladungslinks
- Alle Ansagen (Solo, Hochzeit, Armut, Schmeißen, ...)
- Sonderpunkte (Doppelkopf, Fuchs gefangen, Karlchen, Gegen die Alten, ...)
- Alle Vorbehalte / Spielmodi (Kontrasolo, Schlanker Martin, ...)
- Ergebnis-Anzeige mit Stichen und Augen pro Spieler
- PWA-Unterstützung (Installation als App)
