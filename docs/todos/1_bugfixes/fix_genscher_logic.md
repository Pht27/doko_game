Ich mag nicht, wie Genschern in der Application / Domain gehandlet wird:

Der PlayCardCommand kommt mit einem potentiellen Genscherpartner, das ist okay. Aber dass der game State im PlayCardHandler modifiziert wird ist falsch: Der GameState wird nur von sich selbst durch Apply von GameStateModifications verändert - bei der Code Recherche hatte ich keine Chance in der Domain der Genscher Effekt zu finden, weil er nicht auftaucht.

Da muss man nochmal darüber nachdenken, wie man das handlet. Problematisch ist schon, dass die Apply Methode keinen extra parameter nimmt. Das könnte man vielleicht ändern, indem man da irgendwie Parameter einfügt, allerdings wäre es auch doof wenn da jede Sonderkarte eine GenscherID in den Parametern stehen hätte... Kann man irgendwie über generische Parameter gehenm also sowas wie Parameter<T> wobei T eine Sonderkarte ist?

Da muss man sich nochmal den Kopf zerbrechen aber so wie es ist kann es nicht bleiben