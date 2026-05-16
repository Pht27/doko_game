# Release Notes

## [1.4.2] - 2026-05-14

### Verbessert
- **RoundForm- und GameBoard-Komponenten** wurden intern in Unterordner aufgeteilt – keine sichtbaren Änderungen für Nutzer, klarere Codestruktur.

### Verbessert
- Die **Farbverläufe für Siegquoten und Ø-Werte** (rot → gelb → grün) passen sich jetzt ans gewählte Theme an – auf hellen Themes wirken die Farben deutlich harmonischer.
- Der **Karten-Rahmen** im Spielerprofil und die **Fehlerfarben** im Namens-Editor nutzen jetzt die Theme-Farben statt fester Werte.

### Neu
- **Sonderkarten und Extrapunkte** zeigen jetzt im Spielerprofil eine aufklappbare **Re/Kontra-Aufschlüsselung**: Wie oft hat man eine Karte/einen Extrapunkt als Re oder Kontra gehabt? Wie war die Siegquote und der Ø-Spielwert je Partei?
- In der **globalen Spielstatistik** (Übersicht → Spielstatistiken) haben Sonderkarten und Extrapunkte jetzt eine zusätzliche Spalte **Re WR**, die zeigt, wie oft Re-Teams mit dieser Karte/diesem Extrapunkt gewonnen haben.

### Neu
- Spieler können jetzt ihr **Profil bearbeiten**: Name ändern und eine **Heldenkarte** aus allen 24 Doppelkopf-Karten wählen. Der ✎-Button neben dem Namen öffnet das Bearbeitungsformular. Die Karte wird dauerhaft gespeichert und im Hero-Bereich des Profils angezeigt.

### Verbessert
- Der **Punkte-Umschalter** im Spielerprofil heißt jetzt „Spielwert / Brutto / Netto" und erklärt per **?-Button** den Unterschied zwischen den drei Punktetypen.
- Auf der **Spielstatistik-Seite** sind die Ø-Spalten jetzt als „Ø Wert" beschriftet, damit klar erkennbar ist, dass dort der Spielwert (ohne Solo- und Teamfaktor) angezeigt wird.

### Verbessert
- Das **Spielerprofil** wurde umfassend überarbeitet: Hero-Bereich mit Karte, Name, Rangabzeichen (Platzierung unter aktiven Spielern) und drei Inline-Stats (Spiele, Winrate, Ø-Wert) direkt neben dem Punktestand. Bestes und schlechtestes Spiel als Mini-Karten (Solos ausgenommen).
- Der **Spielverlauf** im Profil zeigt jetzt den tatsächlich verdienten Punkteunterschied pro Runde (mit Solo- und Teamfaktor). Als Teampartner wird nur der direkte Mitspieler im gleichen Team angezeigt – nicht alle Parteimitglieder. Spielt man alleine, bleibt das Feld leer. Die Partei (Re/Ko) hat eine eigene Spalte.
- Die **Pagination** im Spielverlauf hat jetzt ein klassisches Ellipsis-Format (‹ 1 … 4 5 6 … 12 ›) statt aller Seitenzahlen auf einmal.
- Der **Punkte-Umschalter** (Spielwert / Punkte / Diff) sperrt jetzt nicht anwendbare Optionen: Bei Sonderkarten und Extrapunkten sind nur die relevanten Werte auswählbar.
- Die **Teamstatistik** zeigt jetzt zusätzlich den Ø-Punktewert pro Spiel mit einem bestimmten Partner (mit Solofaktor, ohne Teamfaktor).
- Der Punktestand-Label im **Verlaufs-Graph** hat jetzt einen Pille-Hintergrund, damit er auch bei hellem Graphbereich lesbar bleibt.
- Der Aufklapp-Pfeil bei Spielkarten im Spielverlauf ist jetzt korrekt zentriert.

## [1.4.1] - 2026-05-14

### Behoben
- Beim nachträglichen Bearbeiten eines Spiels bleibt der ursprüngliche Zeitstempel jetzt erhalten – er wurde bisher fälschlicherweise auf die aktuelle Uhrzeit gesetzt.

## [1.4.0] - 2026-05-14

### Neu
- Unter **Übersicht → Spielstatistiken** gibt es jetzt eine neue Seite mit Gesamtstatistiken: Für jeden Spielmodus werden Rundenanzahl, Re-Siegquote und Re-Durchschnittswert angezeigt. Sonderkarten und Extrapunkte sind mit Anzahl, Siegquote und Ø-Wert aufgelistet.
- Das **Spielerprofil** zeigt jetzt detaillierte Spielmodus-Statistiken mit Re/Kontra-Aufteilung – je Spielmodus und Partei eine eigene Zeile mit Spiele, Siegquote und Durchschnittswert. Der Punktetyp-Umschalter (Wert / Punkte / Diff) liegt jetzt auf einer eigenen Zeile unter den Tabs, damit er nicht den Tab-Bereich abschneidet. Die Hero-Stats (Spiele, Winrate, Ø) werden jetzt als übersichtliches Drei-Felder-Raster unterhalb des Hero-Bereichs angezeigt.
- Die Datenbankschicht für Spielerstatistiken und Gesamtstatistiken ist jetzt vorbereitet: Neue dbt-Modelle berechnen Siegquoten, Durchschnittspunkte und Bilanzen – aufgeteilt nach Spielmodus, Sonderkarten und Extrapunkten. Für jeden Spieler gibt es eine Gesamtübersicht sowie detaillierte Auswertungen je Partner, Spielmodus und Karte.
- Der dbt-Code liegt jetzt einheitlich unter `Code/database/` – zusammen mit Backend und Frontend im `Code/`-Verzeichnis.


- Beim Eintragen eines neuen Spiels gibt es jetzt einen Button **„Letzte Teams laden"**: Er füllt die vier Team-Blöcke mit den Spielern aus dem zuletzt gespeicherten Spiel – so müssen Spieler nach einem kurzen Zurück-Gehen nicht neu zugewiesen werden. Falls bereits Spieler eingetragen sind, erscheint vorher ein Bestätigungs-Dialog.
- Farbthemen können jetzt direkt auf der Startseite gewechselt werden: Das 🎨-Symbol oben rechts öffnet eine Auswahl mit sechs Themes – **Default** (dunkel), **Rosé**, **Aqua**, **Classic**, **Cherry** und **Nautical**. Die fünf neuen Themes sind helle Designs; jedes zeigt die Kartenfarben Re und Kontra in eigenen Tönen. Die Auswahl wird sofort angewendet und gespeichert.

### Verbessert
- In der Rangliste zeigt das aufgeklappte Detail jetzt zwei Bilanzen nebeneinander: die Gesamtbilanz und die Bilanz der letzten 50 Spiele.
- Beim Speichern einer Runde prüft das System jetzt auf Plausibilität: falsche Spieleranzahl pro Partei (Solo oder Normal), ungültige Sonderkarten-Kombinationen (z. B. Hyperschweinchen ohne Superschweinchen, Heidfrau ohne Heidmann) und überschrittene Limits bei Extrapunkten (z. B. Karlchen + Agathe max. 1×). Fehler werden direkt im Formular angezeigt.

### Behoben
- Der Verlaufs-Graph im Vollbild wurde beim ersten Öffnen nur auf der linken Hälfte angezeigt. Der Graph füllt jetzt sofort den gesamten Bereich aus.
- Die Punkte im Verlaufs-Graph wurden falsch berechnet: Solo-Spiele (3-facher Multiplikator) und die Aufteilung auf Teammitglieder wurden nicht berücksichtigt. Die Punkteverläufe stimmen jetzt mit der Rangliste überein.

## [1.3.7] - 2026-05-10

### Neu
- Die Statistikseite hat jetzt einen interaktiven Verlaufs-Graph im Vollbild: Tippen auf den Chart oder das ⤢-Symbol öffnet eine Querformat-Ansicht. Spieler lassen sich per Toggle-Filter ein- und ausblenden. Ein Schieberegler steuert, wie viele Spiele zurück angezeigt werden – dynamisch bis zum Maximum der vorhandenen Daten. Zwischen Spiel-Index und echten Datumsangaben auf der X-Achse kann umgeschaltet werden. Der Graph unterstützt Pinch-to-Zoom auf beiden Achsen.

### Verbessert
- Die Inline-Komponenten `MultiLineChart` und `ExpandableRow` der Rangliste sind in eigene Dateien ausgelagert – die Codebasis ist damit übersichtlicher strukturiert.
- Die Komponenten `RoundCard` und `TeamBlock` der Rundenübersicht sind in eigene Dateien ausgelagert – die Codebasis ist damit übersichtlicher strukturiert.
- Die Hilfskomponenten `Tile` und `SubItem` der Startseite sind in eigene Dateien ausgelagert. Die CSS-Animationen liegen jetzt in einer eigenen CSS-Datei statt als inline `<style>`-Block.
- Die `SeatCard`-Komponente der Lobby-Detailansicht ist in eine eigene Datei ausgelagert – die Codebasis ist damit übersichtlicher strukturiert.
- Die Komponenten `PlayerRow` und `SectionHeader` der Spielerliste sind in eigene Dateien ausgelagert – die Codebasis ist damit übersichtlicher strukturiert.
- Die URLs und Seitenstruktur für Rundenübersicht, Rangliste, Spielerliste und Spielerdetail sind jetzt allgemein gehalten (`/history`, `/leaderboard`, `/players`) – ohne „analog"-Präfix, da diese Seiten künftig auch digitale Spiele einbeziehen werden.
- Alle deutschen Texte in der App sind jetzt zentral in Übersetzungsdateien gepflegt und nach Bereich aufgeteilt (Spiel, Lobby, Landing, Analog, Regeln). Doppelte und ungenutzte Texte wurden bereinigt.
- Das Einklappen der Untermenüs auf der Startseite läuft jetzt flüssig durch – die Animation startet sofort und ohne Verzögerung.

## [1.3.6] - 2026-05-09

### Neu
- Automatisches Datenbank-Backup: Jede zweite Nacht wird ein komprimiertes Backup der Produktionsdatenbank erstellt und auf dem Server gespeichert. Die letzten 15 Backups werden aufgehoben.

### Verbessert
- Buttons in Dialogen und Formularen sehen jetzt überall gleich aus – Primary-, Sekundär- und Link-Buttons folgen einem einheitlichen Design.
- Lade-, Fehler- und Leer-Zustände sind jetzt einheitlich gestaltet: Ein animierter Spinner zeigt Ladevorgänge an, Fehler erscheinen rot mit Warnzeichen, leere Listen mit dezenter Beschriftung. Formularfehler folgen ebenfalls einem einheitlichen Look.
- Die Sheets für Spielmodus-Auswahl und Team-Editor teilen jetzt eine gemeinsame BottomSheet-Komponente – Overlay, Einfahranimation und Schließen-Button verhalten sich überall identisch.

## [1.3.5] - 2026-05-08

### Neu
- Neue Statistikseite unter „Übersicht → Statistiken": Ein Verlaufs-Chart zeigt die Punkte der Top-Spieler über die letzten 24 Runden. Darunter eine ausklappbare Rangliste – ein Tipp auf eine Zeile zeigt Winrate, Durchschnittspunkte, Höchst- und Tiefstwert samt Sparkline. Sortierung nach Punkten, Winrate oder Spielanzahl per Auswahl. Inaktive Spieler können optional eingeblendet werden. Ein Tipp auf den Spielernamen führt direkt zur Einzelansicht.

### Verbessert
- Spielerstatistiken werden jetzt korrekt berechnet: Punkte werden pro Spieler aufgeteilt wenn zwei Spieler eine Hand teilen, und Solo-Runden zählen dreifach für die Solo-Partei. Die API gibt jetzt zusätzlich Winrate und durchschnittliche Punkte pro Runde zurück.
- Spielerverwaltung komplett überarbeitet: Aktive und inaktive Spieler sind jetzt in zwei getrennten Bereichen. Jede Zeile zeigt ein farbiges Initialen-Avatar, einen Schalter zum Aktivieren/Deaktivieren und ein Stift-Symbol zum Umbenennen direkt in der Liste. Ein Tipp auf den Namen führt weiterhin zur Statistikseite.

## [1.3.4] - 2026-05-08

### Verbessert
- Nochmalige Landing-Page Updates

## [1.3.3] - 2026-05-08

### Verbessert
- Leichte Landing-Page Updates

## [1.3.2] - 2026-05-08

### Neu
- Wenn ein App-Update bereitsteht, zeigt der Versionsknopf unten einen Hinweis an. Ein Tipp darauf öffnet die Release Notes – mit einem „Aktualisieren"-Button, der die neue Version direkt installiert. Kein manuelles Cache-Leeren mehr nötig.

### Verbessert
- Startseite neu geordnet: Alle vier Kacheln klappen jetzt auf. ♣ zeigt Spiel eintragen & Spieler (blau), ♠ Mehrspieler & Testspiel, ♥ Rundenübersicht & Statistiken, ♦ Regeln & Regelsets (orange). Die Optionen im aufgeklappten Menü sind jetzt durch eine Trennlinie klar voneinander abgegrenzt.

## [1.3.1] - 2026-05-07

### Behoben
- Beim Spiel eintragen kann eine Sonderkarte nur noch einem Team zugewiesen werden – war sie bereits beim anderen Team, taucht sie dort nicht mehr zur Auswahl auf.
- Beim Öffnen des Team-Editors wird das Spieler-Suchfeld nicht mehr automatisch fokussiert.

### Verbessert
- Wechselt ein Team per Wisch-Geste die Seite (Re ↔ Kontra), gleitet der Block jetzt animiert in die neue Spalte.

## [1.3.0] - 2026-05-05

### Neu
- Neue Seiten für den Analog-Modus: Unter `/analog/players` gibt es eine Spieler-Übersicht mit Gesamtpunkten, Spielen, Siegen und Niederlagen. Ein Klick auf einen Spieler zeigt das Einzelprofil mit der Rundenhistorie. Neue Spieler können direkt über die Übersicht angelegt werden.
- Rundenhistorie unter `/analog/history`: Alle eingetragenen Runden, neueste zuerst, mit Datum, Spielern, Ergebnis und Spielmodus. Runden können direkt gelöscht werden. Weitere Runden werden per „Mehr laden" nachgeladen.
- Spiel eintragen: Über „Spiel eintragen" auf der Startseite können Runden mit Spielmodus, Punkte, Siegerpartei, Sonderkarten und Extrapunkten eingetragen werden. Teams werden per Wisch-Geste zwischen Re und Kontra gewechselt. Bestehende Runden können über die Rundenhistorie bearbeitet werden.

## [1.2.2] - 2026-05-02

### Neu
- Zu Beginn jedes Spiels läuft eine kurze Misch- und Austeilanimation: Karten kreisen in der Mitte, der Stapel wird abgehoben und dann fliegen die Karten zu jedem Spieler.
- Spieler können sich in der Lobby einen eigenen Namen geben (Stift-Symbol auf dem eigenen Sitz). Der Name ist für alle anderen Mitspieler sichtbar und erscheint überall, wo bisher „S1"–„S4" stand: Spielfeld, Dialoge, Ergebnisanzeige.

## [1.2.1] - 2026-05-01

### Spielfläche
- Die Gesamtstichanzahl wird nicht mehr angezeigt.
- Die Auswahl des Genscher-Partners zeigt die Spieler jetzt in ihrer Tisch-Position an (oben, links, rechts). Der Dialog wurde dafür etwas verbreitert. Der Titel lautet jetzt „Wohin genschern?".
- Die Sonderkarte-Animation zuckt am Ende nicht mehr hin und her.

## [1.2.0] - 2026-05-01

### Spielfläche
- Im Spiel werden jetzt die Vorbehalte und Spielphasen besser angezeigt.
- Aktive Sonderkarten werden angezeigt.
- Beim Ausspielen einer Sonderkarte wird klar angesat, was ausgespielt wurde.

### Behoben
- Beim Genschern konnte es passieren, dass der Dialog ohne Auswahl bestätigt wurde und ein Fehler auftrat. Das ist jetzt behoben.
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
