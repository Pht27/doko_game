# Doppelkopf — Game Rules Reference

This document is the authoritative game rules reference for implementation.
It will be expanded incrementally as features are specified.

## Overview

Doppelkopf is a trick-taking card game for 4 players using a 48-card deck
(a standard 32-card German deck doubled — each card appears twice).

## Players & Teams

- 4 players per round
- Two teams of 2: **Re** (holding Queens of Clubs) vs **Kontra**
- Team membership is secret at the start

## Deck

48 cards: two copies of each of the following 24 cards:
- Suits: Clubs (♣), Spades (♠), Hearts (♥), Diamonds (♦)
- Ranks: 9, J (Bube), Q (Dame), K (König), 10, A

## Trump Cards (Trumpf)

Fixed trump order (highest to lowest):

1. 10 of Hearts (Dulle / Tolle) — *two copies*
2. Q of Clubs (Kreuz-Dame) — *two copies, marks Re team*
3. Q of Spades (Pik-Dame)
4. Q of Hearts (Herz-Dame)
5. Q of Diamonds (Karo-Dame)
6. J of Clubs (Kreuz-Bube)
7. J of Spades (Pik-Bube)
8. J of Hearts (Herz-Bube)
9. J of Diamonds (Karo-Bube)
10. A of Diamonds (Karo-As)
11. 10 of Diamonds
12. K of Diamonds
13. Q of Diamonds *(already listed as trump — Diamonds suit is fully trump)*
14. 9 of Diamonds

> Note: All Diamonds, all Jacks, all Queens, and the 10 of Hearts are trump.
> Remaining suits (Clubs, Spades, Hearts) are plain suits.

## Card Values (Augen)

| Rank | Points |
|------|--------|
| A    | 11     |
| 10   | 10     |
| K    | 4      |
| Q    | 3      |
| J    | 2      |
| 9    | 0      |

Total points per full deck: **240**

## Dealing

- Each player receives 12 cards
- Dealing rotates clockwise each round

## Gameplay

- 12 tricks per round
- Leader of each trick plays first; others must follow suit if able
- Trick winner leads the next trick
- Trump beats any non-trump card
- Within a suit/trump, higher rank wins; in case of equal rank, first played wins

## Scoring

- Re team needs **121+ points** to win (majority of 240)
- Kontra team wins with 120+ points (Re did not reach 121)
- Base game value: 1 point per player (to be expanded with Sonderpunkte)

## Variants & Special Rules

To be documented as features are implemented:
- Hochzeit (Marriage)
- Armut (Poverty)
- Solo
- Announcements (Re, Kontra, Keine 90, etc.)
- Sonderpunkte (Fuchs, Karlchen, Doppelkopf, etc.)
