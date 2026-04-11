Es sollte einmal gecheckt werden, ob Kontrasolo und stille Hochzeit schon implementiert sind. Bei Kontrasolo ist die starke Vermutung nein, es ist nirgends im Repo.

Bei zwei Kreuzdamen auf der Hand hat man die Wahl, ob man Hochzeit ansagt oder sie still spielt. Beim Kontrasolo wird hier festgelegt: Es ist Pflicht! (Bitte in den Regeln ergänzen) Man hat also, wenn man diese vier Karten auf der Hand hat keine Wahl, als das Kontrasolo zu spielen.

Die Regeln für das Kontrasolo stehen recht gut beschrieben, sie müssen dann nur noch implementiert werden und es muss einen Trumpresolver geben.

Für die stille Hochzeit muss nochmal Code Recherche betrieben werden. Wie der Stand da ist.

Vielleicht sollte es sowas wie SilentReservations geben? Damit könnte im Frontend auch immer easy die ActiveReservation angezeigt werden, ohne das stille Solo zu verraten. Dann muss man sich überlegen, wie ActiveReservation und SilentReservation zusammenhängen / gekoppelt sind.
---

Bei einer stillen Hochzeit steht in den Regeln, dass keine der Genscherdamen aktiviert werden kann. Das ist unpräzise und muss geupdated werden: Damit ein verhindertes Aktivieren der (Gegen-)Genscherdamen nicht die stille Hochzeit verrät, falls diese bei einem der Kontra-Spieler liegen, müssen die Genscherdamen nach wie vor aktivierbar sein. Allerdings dürfen sie am Ende des Spiels keine Auswirkungen haben, da feststeht, wie im stillen Solo die Parteien sind und das hat Vorrang. Da muss man genau in die PartyResolver Logik schauen! Bitte auch in den Regeln ergänzen.

--- 

In dem seltenen Fall, dass man ein Kontrasolo und eine Hochzeit auf der Hand hat (also beide Kreuz und Pik Damen und beide Pik Könige), gibt es folgende Optionen:

1. Man meldet einen Vorbehalt an, der nicht Hochzeit ist. Dann wird dieser Vorbehalt gespielt
2. Man meldet eine Hochzeit an. Dann wird ganz normal Hochezit gespielt mit Findungsstich etc. und die Kontrasoloregeln (man ist alleine Kontra, die beiden Könige sind die stärksten Karten etc.) gelten nicht
3. Man meldet keinen Vorbehalt an. Hier ist jetzt unklar, ob man eine stille Hochzeit oder ein Kontrasolo spielt. Nach Konvention spielt man dann automatisch ein Kontrasolo! Also ist man die Kontra Partei und die beiden Pikkönige sind keine Pikkarten sondern Trumpfkarten (über ALLEN anderen Karten also auch Schweinchen etc.).