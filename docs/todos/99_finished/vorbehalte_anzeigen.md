# Vorbehalte-Dialog: Zwei-Spalten-Auswahl

## Ausgangslage

Der aktuelle `ReservationDialog` zeigt alle möglichen Vorbehalte als flache Liste von Buttons. Das ist unübersichtlich, wenn viele Optionen verfügbar sind (z.B. 8 Solos + Armut + Hochzeit).

## Ziel

Zwei-Spalten-Auswahl:
- **Linke Spalte**: Kategorie wählen (Solo | Schlanker Martin | Armut | Schmeißen | Hochzeit) – nur anzeigen wenn jeweils mindestens eine Option verfügbar
- **Rechte Spalte**: Detailauswahl je nach Kategorie:
  - **Solo** → Liste der verfügbaren Solos mit deutschen Namen
  - **Hochzeit** → 3 Buttons für die Findungsbedingung (Erster Stich / Erster Fehlstich / Erster Trumpfstich)
  - **Schlanker Martin / Armut / Schmeißen** → direktes Deklarieren per einzelnem "Bestätigen"-Button

## Kategorie-Zuordnung (Backend-Enums → Kategorie)

| Backend-String | Kategorie |
|---|---|
| `KaroSolo`, `KreuzSolo`, `PikSolo`, `HerzSolo`, `Damensolo`, `Bubensolo`, `Fleischloses`, `Knochenloses` | Solo |
| `SchlankerMartin` | Schlanker Martin |
| `Armut` | Armut |
| `Schmeissen` | Schmeißen |
| `Hochzeit` | Hochzeit |

## Deutsche Namen für Solos

| Backend-String | Anzeigename |
|---|---|
| KaroSolo | Karo-Solo |
| KreuzSolo | Kreuz-Solo |
| PikSolo | Pik-Solo |
| HerzSolo | Herz-Solo |
| Damensolo | Damen-Solo |
| Bubensolo | Buben-Solo |
| Fleischloses | Fleischloses |
| Knochenloses | Knochenloses |

## UX-Verhalten

- Erste verfügbare Kategorie ist standardmäßig ausgewählt (linker Tab highlighted)
- "Passen"-Button oben (entfällt wenn `mustDeclare=true`)
- Mobile-first: volle Breite, `max-w-sm`, Touch-freundliche Button-Größen

## Betroffene Dateien

1. `src/frontend/src/components/dialogs/ReservationDialog.tsx` – Komplette Überarbeitung
2. `src/frontend/src/styles/ReservationDialog.css` – Neue Zwei-Spalten-Styles
3. `src/frontend/src/translations.ts` – Deutsche Label für Kategorien und Solo-Namen

## Layout-Skizze

```
+─────────────────────────────────────────+
│  S1: Ansagen              [Passen]       │
+──────────────+──────────────────────────+
│ ● Solo       │  Karo-Solo               │
│   Hochzeit   │  Kreuz-Solo              │
│   Armut      │  Pik-Solo                │
│              │  Herz-Solo               │
│              │  Damen-Solo              │
│              │  Buben-Solo              │
│              │  Fleischloses            │
│              │  Knochenloses            │
+──────────────+──────────────────────────+
```

Keine Backend-Änderungen notwendig – nur Frontend.
