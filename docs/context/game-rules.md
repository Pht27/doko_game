# Koppeldopf — Game Rules Reference

This document is the authoritative rules reference for implementation.
It describes the **custom "Koppeldopf" ruleset** — a variant of Doppelkopf.

Rules marked **[CONFIGURABLE]** are optional game settings that can be toggled.
Rules marked **[CORE]** are always active (the minimal playable game).

---

## Overview

Koppeldopf is a trick-taking card game for **4 players** using a **48-card deck**
(a standard 32-card German deck doubled — each card appears twice).
Play direction is **counterclockwise** (mathematisch positiv).

---

## Deck & Card Categories

48 cards: two copies each of 24 distinct cards.
Suits: ♣ Kreuz (Clubs), ♠ Pik (Spades), ♥ Herz (Hearts), ♦ Karo (Diamonds).
Ranks: 9, J (Bube), Q (Dame), K (König), 10, A.

Cards belong to one of four categories:
- **Trumpf** — beats all Fehl cards; see trump order below
- **Kreuz** (plain) — ♣ suit minus those that are trump
- **Pik** (plain) — ♠ suit minus those that are trump
- **Herz** (plain) — ♥ suit minus those that are trump

Kreuz, Pik, Herz as plain suits are collectively called **"Fehl"** or **"Fehlfarben"**.
♦ Karo has no plain-suit cards in the normal game — all ♦ cards are trump.

**[CONFIGURABLE]** Play with 9s. When disabled, 9s are removed from the deck (40 cards total, 10 per player).

---

## Trump Order (Normal Game) [CORE]

Highest to lowest:

| # | Card | Name |
|---|------|------|
| 1–2 | ♥ 10 (×2) | Dulle / Tolle |
| 3–4 | ♣ Q (×2) | Kreuz-Dame — marks **Re** party |
| 5–6 | ♠ Q (×2) | Pik-Dame |
| 7–8 | ♥ Q (×2) | Herz-Dame |
| 9–10 | ♦ Q (×2) | Karo-Dame |
| 11–12 | ♣ J (×2) | Kreuz-Bube |
| 13–14 | ♠ J (×2) | Pik-Bube |
| 15–16 | ♥ J (×2) | Herz-Bube |
| 17–18 | ♦ J (×2) | Karo-Bube |
| 19–20 | ♦ A (×2) | Karo-Ass (Fuchs) |
| 21–22 | ♦ K (×2) | Karo-König |
| 23–24 | ♦ 10 (×2) | Karo-Zehn |
| 25–26 | ♦ 9 (×2) | Karo-Neun |

> All Queens, all Jacks, the ♥ 10, and all ♦ cards are trump.
> ♣, ♠, ♥ (minus their Jacks, Queens, and the ♥ 10) are plain (Fehl) suits.

### Dulle Rule [CONFIGURABLE]
Default: **second Dulle beats the first** — except in the **last trick**, where the first Dulle beats the second (reversed).

Configurable options:
- `SecondBeatsFirst` *(default)* — second Dulle wins; reversed in last trick
- `FirstBeatsSecond` — first Dulle always wins (no last-trick exception)
- `Equal` — Dullen are equal; first played wins (standard tie-breaking, no exception)

### Tie-breaking within equal cards [CORE]
For identical cards: **first played wins**, with the single exception of the Dulle (see above).

---

## Fehl Suit Order (plain suits, normal game) [CORE]

Within a plain suit, rank order high to low:

**♣, ♠:** A > 10 > K > 9
**♥:** A > K > 9 *(♥ 10 is trump, not a plain Herz card)*

Queens and Jacks of these suits are all trump — they do not appear as plain cards.

---

## Card Values (Augen) [CORE]

| Rank | Points |
|------|--------|
| A    | 11     |
| 10   | 10     |
| K    | 4      |
| Q    | 3      |
| J    | 2      |
| 9    | 0      |

Total points in the full deck: **240**.

---

## Dealing [CORE]

- Cards are dealt **4 × 3** per player (four rounds of 3 cards each), giving each player 12 cards.
- The **dealer rotates** each round.
- The player to the **right** of the dealer leads reservations and the first trick.

---

## Parties (Parteien) [CORE]

- **Re**: the two players holding the ♣ Queens. Need **121+ points** to win.
- **Kontra**: the remaining two players. Win if Re does not reach 121 (i.e., ≥ 120 suffices).
- Party membership is **secret** at the start (except when revealed by play or announcements).
- Each player in a party wins or loses **together**; both get the same score written.

---

## Basic Gameplay [CORE]

1. The player to the right of the dealer **leads** the first trick (plays any card).
2. Each subsequent player must **follow suit** if able (trump to trump, Fehl suit to same Fehl suit).
3. If unable to follow suit, any card may be played.
4. The **highest trump** wins a trick containing trump; otherwise the **highest card of the led suit** wins.
5. The winner of a trick **leads the next trick**.
6. After 12 tricks, each party counts its **Augen**. Re wins with 121+.

---

## Reservations (Vorbehalte) [CORE structure, individual items CONFIGURABLE]

Before play begins, reservations are declared in **lead order** (starting with the player who leads first).
A player with a reservation says "Vorbehalt"; one without says "Gesund".
Reservations are then revealed in the same order.

**Priority order** (highest first — higher priority pre-empts lower):

1. **Solo** [CONFIGURABLE] — sub-priority by solo type and color order (♣ > ♠ > ♥ > ♦ for Farbsoli); **Schlanker Martin has the lowest sub-priority within Soli**
2. **Armut** [CONFIGURABLE]
3. **Schwarze Sau** [CONFIGURABLE] — only triggered if Armut is not accepted
4. **Hochzeit** [CONFIGURABLE]
5. **Schmeißen** [CONFIGURABLE]

If two players have the **same reservation**, lead order decides who plays it.

---

## Schmeißen (Redeal) [CONFIGURABLE]

A player may declare Schmeißen (forcing a redeal) if their hand meets **any** of:

- Total Augen **> 80**
- Total Augen **< 35**
- **≤ 3 trump cards**
- Highest trump is a **♦ Jack**
- At least **5 Nines**, at least **5 Kings**, or the **sum of Nines + Kings ≥ 8**

---

## Armut (Poverty) [CONFIGURABLE]

- Condition: **≤ 3 trump cards**, where **♦ Aces (Füchse) are not counted**.
- In lead order starting from the player after the poor player, others are asked if they accept.
- The accepting player ("reiche Spieler*in") receives **all of the poor player's trump**, inspects them, and returns **the same number** of cards of their choice (may include just-received cards).
- Must openly communicate: **how many** cards were exchanged, and **whether trump was returned**.
- The poor + rich player form the **Re party**.
- The first opposing player **left of the rich player** leads.
- **All Sonderkarten are deactivated** in Armut.
- If **nobody accepts**, a **Schwarze Sau** is played instead.

---

## Schwarze Sau [CONFIGURABLE]

Triggered when Armut is declined by all.

- Play proceeds as a normal game with Sonderkarten active.
- The player who **wins the trick containing the second ♠ Queen** is forced to choose and play a **Solo** with their remaining cards from that point on. Normal Solo rules then apply (Sonderkarten deactivated). The player may choose any configured Solo type **except** Kontrasolo and Stille Hochzeit.
- The Armut player leads.

---

## Hochzeit (Marriage) [CONFIGURABLE]

- Condition: a player holds **both ♣ Queens**.
- The player announces Hochzeit and names a condition: **"first trick"**, **"first Fehl trick"**, or **"first trump trick"** (a trick is a Trumpf/Fehl trick based on the **led card**).
- The player who wins the described trick becomes the **second Re party member**.
- If no partner is found in the **first three tricks**, the player plays a **Stille Hochzeit** (solo) for the rest of the game.
- Announcements are only allowed **after the partner is found**. The window opens at that moment and closes before the **second card played after marrying**; each further announcement extends the deadline by one card, as usual.
- If no partner is found in 3 tricks (→ Stille Hochzeit), the party was never determined and no announcements were possible.

---

## Stille Soli [CONFIGURABLE]

Silent solos are not declared as reservations but revealed through play.

### Stille Hochzeit
- A player with both ♣ Queens may play silently (not declare Hochzeit).
- Plays like a normal game; only the Hochzeit party knows it is effectively a solo.
- Revealed when the second ♣ Queen is played.
- Announcements and all normal rules apply, **except** Genscherdamen and Gegengenscherdamen have no effect on party determination — the stille solo party structure takes precedence at game end. However, Genschern/Gegengenschern **can still be announced** and the passive Re status from holding both ♥ Queens still applies; this prevents a Kontra player holding both ♥ Queens from inadvertently revealing the solo by failing to announce.
- Scored as a **Solo** at the end.

### Kontrasolo [CONFIGURABLE]
- Condition: player holds **both ♠ Queens AND both ♠ Kings**.
- When the condition is met (without also holding both ♣ Queens), playing Kontrasolo is **mandatory** — the player has no choice.
- Plays like a Stille Hochzeit; the allowed announcement is **"Kontra"** (regardless of ♣ Queens held).
- The ♠ Kings become **Klabautermänner** — the **highest trumps** in the game (above all Sonderkarten). Announced by calling **"Klabautermann"** when playing one, which also reveals the solo.
- Committing action (no changing one's mind after): playing/not-playing the ♠ Kings in a way that would reveal the solo, or announcing "Kontra"/"Re" while holding ♣ Queens.
- If a player qualifies for both Stille Hochzeit and Kontrasolo (holds both ♣ Queens, both ♠ Queens, and both ♠ Kings), three options apply:
  1. Declare a **non-Hochzeit reservation** → that reservation is played.
  2. Declare **Hochzeit** → normal Hochzeit with Findungsstich etc.; Kontrasolo rules do not apply.
  3. **No declaration** → Kontrasolo by convention.

### Stille Armut [CONFIGURABLE — PRELIMINARY / WORK IN PROGRESS]
- A player with an Armut hand may choose to play it silently.
- A normal game is played; the Armut party's goal is to **win no tricks**.
- If successful and the player with the most points has **> 90**, the Armut party wins a solo worth 1 point. For every 30 points above 91, the game is worth 1 additional point.
- *Status: design is not finalised.*

---

## Soli [CONFIGURABLE per type]

In **all Soli**: Sonderkarten and Extrapunkte are deactivated — **except in Stille Soli** (Stille Hochzeit, Kontrasolo), where both Sonderkarten and Extrapunkte remain active. The solo player **always leads**. The solo player's points are **tripled** when recording scores. Base game value is 1, increasable by announcements.

Color order (canonical): ♣ > ♠ > ♥ > ♦.

### Farbsolo [CONFIGURABLE]
Four variants: ♣, ♠, ♥, ♦ Solo.
- Same trump structure as normal, but the **♦ trump suit is replaced by the chosen color**.
- ♥ 10 (Dulle) remains the highest trump in all Farbsoli.
- ♥ Solo has fewer trumps (no ♥ 10 as a plain card, but it still exists as Dulle).
- Goal: **121 Augen**. All announcements allowed.
- Priority follows color order.

### Damensolo [CONFIGURABLE]
- Only **Queens** are trump (4 × 2 = 8 cards).
- All other cards are plain. Suit order: A > 10 > K > J > 9 (Jacks below Kings, tens are high).
- Goal: **121 Augen**. All announcements allowed.

### Bubensolo [CONFIGURABLE]
- Only **Jacks** are trump (4 × 2 = 8 cards).
- All other cards are plain. Suit order: A > 10 > K > Q > 9 (Queens below Kings, tens are high).
- Goal: **121 Augen**. All announcements allowed.

### Fleischloses (Nullo) [CONFIGURABLE]
- **No trump**. All cards are plain.
- Suit order: A > 10 > K > Q > J > 9 (tens are high, Jacks below Queens, Queens below Kings).
- Goal: **121 Augen**. All announcements allowed.

### Knochenloses [CONFIGURABLE]
- **No trump**. All cards are plain.
- Suit order: A > K > Q > J > 10 > 9 (**tens are low**, just above 9s).
- Goal: **win no tricks**. Game ends immediately when the solo player wins a trick (solo player loses).
- No announcements.

### Schlanker Martin [CONFIGURABLE]
- Normal game rules, but no Sonderkarten and no Extrapunkte.
- **Tie-breaking reversed**: second identical card beats the first — including Dullen (♥ 10); the configured Dulle rule and last-trick exception do **not** apply.
- Goal: **fewest tricks** for the solo player.
- Draw: if the solo player ties for fewest tricks with another player, the game is worth **0 points**.
- No announcements.
- Schlanker Martin has the **lowest sub-priority within Soli**.

---

## Announcements (Ansagen) [CONFIGURABLE]

Players may announce to raise the stakes. Announcements are **"Re"** or **"Kontra"** (depending on party), followed optionally by point thresholds.

**Available announcements in order:**
1. "Re" / "Kontra"
2. "Keine 90" (opponent won't reach 90)
3. "Keine 60"
4. "Keine 30"
5. "Schwarz" (opponent wins no tricks — not "0 points")

Each announcement raises the **game value by 1**.

**Timing:** Announcements are allowed until **before the second card of the second trick**.
Each announcement made **shifts this deadline forward by one full trick** for all players.

**Consecutive announcements:** Each announcement must be the next level up from the last by the same party (e.g., "Keine 90" requires "Re"/"Kontra" to already have been announced by that party).

---

## Pflichtansage (Mandatory Announcement) [CONFIGURABLE]

- If the **first trick is worth ≥ 35 Augen**, the player who wins it **must** announce ("Re" or "Kontra").
- If the **second trick is also ≥ 35 Augen**, the winner must announce consecutively (e.g., "Keine 90" if same party, or "Kontra"/"Re" if other party).
- Treated as a regular announcement for scoring.
- **Does not apply in Soli.**

---

## Feigheit (Cowardice) [CONFIGURABLE]

If a party wins but was "cowardly" (did not announce aggressively enough relative to the margin), they actually **lose**.

Rule: The **winning party** must not be missing **more than 2 announcements** relative to how badly the losing party lost.

Formally (translation-invariant): if nothing was announced and the losing party has **< 60 Augen**, the winning party loses (they are missing "Re"/"Kontra", "Keine 90", and "Keine 60" — 3 missing announcements).

Each additional missing announcement beyond 2 **loses the game** and adds **1 extra minus point**.

Examples:
- Nothing announced, loser has < 60: loser party actually wins (3 missing announcements).
- Nothing announced, loser has < 30: loser wins and game is worth **2 extra points**.
- "Re" announced only, loser has < 30: loser wins (missing "Keine 90" and "Keine 60").

**Does not apply in Soli** (including Stille Soli).

---

## Sonderkarten (Special Cards) [CONFIGURABLE — all off in Armut and all Soli]

All Sonderkarten are **deactivated in Soli and Armut** — **except in Stille Soli** (Stille Hochzeit, Kontrasolo), where both Sonderkarten and Extrapunkte remain active. Each is individually configurable.

### Schweinchen [CONFIGURABLE]
- Condition: a player holds **both ♦ Aces**.
- These become **Schweinchen**, ranking **above the Dullen** (♥ 10) — the two highest trumps.
- May be announced when playing the **first Schweinchen**.
- If not announced, they can still be captured as Füchse.

### Superschweinchen [CONFIGURABLE]
- Requires Schweinchen to be active (either announced or second ♦ Ace played).
- Condition: a player holds **both ♦ 10s** (on one hand).
- These become **Superschweinchen**, ranking **above the Schweinchen**. When active, the ♦ 10s are removed from their normal trump position (rank 23–24) and placed above the Schweinchen.
- A ♦ 10 played before Schweinchen were announced can still be a Superschweinchen when the second is played.
- If Schweinchen were not announced but a player plays the second ♦ Ace (having had both), Superschweinchen may be announced from that point.

### Hyperschweinchen [CONFIGURABLE]
- Requires Superschweinchen to be active.
- Condition: a player holds **both ♦ Kings** (on one hand).
- These become **Hyperschweinchen**, ranking **above the Superschweinchen**. When active, the ♦ Kings are removed from their normal trump position (rank 21–22) and placed above the Superschweinchen.
- Same propagation rule as Superschweinchen (one card already played still allows activation).

### Linksdrehender Gehängter [CONFIGURABLE]
- Condition: a player holds **both ♦ Jacks**.
- When playing the **first ♦ Jack**, may be announced as "Linksdrehender Gehängter" — allows the player to **reverse the play direction** (counterclockwise ↔ clockwise).
- When playing the **second ♦ Jack**, may announce "Rechtsdrehender Gehängter" — allows reversing the direction again.
- Direction change takes effect **immediately** if the card is led; otherwise from the next trick.
- Announcement and direction change are each **optional individually**.

### Genscherdamen [CONFIGURABLE]
- Condition: a player holds **both ♥ Queens**.
- When playing the **first ♥ Queen**, may announce "Genschern" — the player **chooses a new partner**. The new pair becomes the **Re party**. The second ♥ Queen has no additional effect.
- When Genschern is announced: all prior **announcements forfeit**; no Feigheit applies.
- Retroactive captures: Füchse, Klabautermänner, and caught Gänse are counted for whoever holds them at the point of Genschern.
- If Genscherdamen are not announced, the player with both ♥ Queens is **still Re** (due to potential to Genschern), but no announcements forfeit.

### Gegengenscherdamen [CONFIGURABLE]
- Requires Genscherdamen to be active (either announced or the second ♥ Queen played).
- Condition: a player holds **both ♦ Queens**.
- After Genschern (or after the second ♥ Queen from the same hand), may counter-genschern when playing the first ♦ Queen: **choose a new partner**.
- Same rules apply: Gegengenscherpartei becomes Re; announcements forfeit; retroactive captures apply.
- If not announced: still Re due to potential.
- If Gegengenschern restores the original parties, prior announcements and Feigheit rules **are reinstated**.

### Heidmann [CONFIGURABLE]
- Condition: a player holds **both ♠ Jacks**.
- When playing the **first ♠ Jack**, may announce "Heidmann" — **Jacks now rank above Queens** in the trump order.
- Must announce on the first ♠ Jack; if not announced then, the effect **expires permanently**.

### Heidfrau [CONFIGURABLE]
- Requires Heidmann to have been announced.
- Condition: a player holds **both ♠ Queens**.
- After Heidmann announcement, when playing the next ♠ Queen, may choose to **reverse the Heidmann effect** (Queens back above Jacks).

### Kemmerich [CONFIGURABLE]
- Condition: a player holds **both ♥ Jacks**.
- When playing **either ♥ Jack**, may **withdraw one announcement**, making it invalid.
- Can only withdraw announcements made by **own party** — but cannot withdraw a partner's announcement unless **both** party members have already announced.
- Only **one announcement total** can be withdrawn (not one per Jack).
- Pflichtansagen can also be withdrawn.
---

## Extrapunkte (Bonus Points) [CONFIGURABLE — all off in Soli]

All Extrapunkte are **deactivated in Soli** — **except in Stille Soli** (Stille Hochzeit, Kontrasolo), where they remain active. Each is individually configurable.
Extra points from both parties are **offset against each other** in the final score.

### Doppelkopf [CONFIGURABLE]
- A trick worth **≥ 40 Augen** gives the winning party **+1 point**.

### Fuchs gefangen [CONFIGURABLE]
- The ♦ Aces are **Füchse** (unless upgraded to Schweinchen).
- Each Fuchs that ends up with the **opposing party** gives that party **+1 point**.

### Karlchen [CONFIGURABLE]
- The ♣ Jacks are **Karlchen**.
- A Karlchen winning the **last trick** gives the winning party **+1 point**.
- **Deactivated** when Heidmann is announced; **reactivated** when Heidfrau reverses it.

### Agathe [CONFIGURABLE]
- Both ♦ Queens are the **Agathe**.
- If a Karlchen (♣ Jack) is beaten by an Agathe (♦ Queen) in the **last trick**, and that ♦ Queen wins the trick, and the Karlchen **belonged to the opposing party**, the Agathe's party gets **+1 point**.
- Functions independently of Gegengenscherdamen.

### Fischauge [CONFIGURABLE]
- Both ♦ Nines become **Fischaugen** after the **first trump card is played**.
- By canon: when activated, a Fischauge is **placed face-down on the table**.
- If a Fischauge **wins a trick**, the winning party gets **+1 point**.

### Gans gefangen [CONFIGURABLE]
- If a Fischauge is beaten by **exactly one Fuchs** (and that Fuchs wins the trick), the Fuchs **"steals the Gans"**.
- If the Fischauge belonged to the **opposing party**, the Fuchs's party gets **+1 point**.

### Festmahl [CONFIGURABLE]
- If a trick contains **≥ 3 animals** and **at least two are the same type**, the **second card of the majority type** (in play order) wins the trick.
- Festmahl only applies if Blutbad does not (Blutbad takes precedence).
- If there are exactly **two pairs**, the **last card played** wins.
- Animals: Fischaugen + all members of the Schweinchen family (Schweinchen, Superschweinchen, Hyperschweinchen).

### Blutbad [CONFIGURABLE]
- If a trick contains **≥ 3 different animal types**, the **non-animal card** wins the trick.
- If all cards in the trick are animals, the **Fischauge** wins.
- Blutbad takes precedence over Festmahl when both conditions are met.
- Animals: same as Festmahl.

### Klabautermann [CONFIGURABLE]
- If a **♠ King** is captured by a **♠ Queen** (the ♠ Queen wins the trick and the ♠ King belonged to the **opposing party**), the ♠ Queen's party gets **+1 point**.

### Kaffeekränzchen [CONFIGURABLE]
- A trick consisting of **4 Queens** (any suits) gives the winning party **+1 point**.

---

## Game Value (Spielwert) [CORE structure]

The game value is built up from:

| Component | Points | Condition |
|-----------|--------|-----------|
| Gewonnen | +1 | Winning party |
| Gegen die Alten | +1 | Kontra party wins |
| Keine 90 | +1 | Losing party didn't reach 90 |
| Keine 60 | +1 | Losing party didn't reach 60 |
| Keine 30 | +1 | Losing party didn't reach 30 |
| Schwarz | +1 | Losing party won no tricks |
| Ansage (per announcement) | +1 | Each fulfilled announcement |
| Extrapunkte | ±1 each | Offset between parties |

- Both **winners** record the total as a **positive** score; both **losers** as **negative**.
- In a **Solo**, the solo player's points are **tripled** when recording.

---

## Card Name Reference (German → English)

| German | English |
|--------|---------|
| Ass (A) | Ace |
| König (K) | King |
| Dame (D) | Queen |
| Bube (B) | Jack |
| Kreuz (♣) | Clubs |
| Pik (♠) | Spades |
| Herz (♥) | Hearts |
| Karo (♦) | Diamonds |
| Trumpf | Trump |
| Fehl / Fehlfarbe | Plain suit |
| Stich | Trick |
| Augen | Points (card pip values) |
| Ansage | Announcement |
| Vorbehalt | Reservation |
| Gesund | No reservation |
