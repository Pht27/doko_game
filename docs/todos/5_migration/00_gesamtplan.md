# Migration Gesamtplan

Vollständige Übersicht aller Migrations-Schritte. Details jeweils in den verlinkten Einzel-Todos.

> Hintergrund & Motivation: [3_new_features/migration.md](../3_new_features/migration.md)

---

## Schritte

| # | Todo | Status | Abhängigkeit |
|---|------|--------|-------------|
| 0 | [Server Setup](99_finished/01_server_setup.md) | erledigt | – |
| 1 | [Doko.Analog Projekt + DB Schema](99_finished/02_doko_analog_projekt.md) | erledigt | – |
| 2 | [React Router + Layout-Splitting](99_finished/03_react_router.md) | erledigt | – |
| 3 | [Orientation Lock Refactoring](99_finished/04_orientation_lock.md) | erledigt | 2 |
| 4 | [Analog API – Spieler](05_analog_api_spieler.md) | offen | 1 |
| 5 | [Analog API – Runden](06_analog_api_runden.md) | offen | 1 |
| 6 | [Spiel-Eintragen Analyse](07_spiel_eintragen_analyse.md) | offen | – |
| 7 | [Frontend – Spieler-Seite](08_frontend_spieler.md) | offen | 2 + 4 |
| 8 | [Frontend – Match-History](09_frontend_history.md) | offen | 2 + 5 |
| 9 | [Frontend – Spiel-Eintragen](10_frontend_spiel_eintragen.md) | offen | 2 + 5 + 6 |
| 10 | [Datenmigration MySQL → PostgreSQL](11_datenmigration.md) | offen | 1 |

Schritte 0, 1 und 2 können parallel gestartet werden. Schritt 6 (Analyse) kann jederzeit erledigt werden.

---

## Architektur-Überblick

```
Doko.sln
├── Doko.Domain           Spiellogik (unverändert)
├── Doko.Application      Spiellogik (unverändert)
├── Doko.Infrastructure   In-Memory-Repos (unverändert)
├── Doko.Analog           NEU: Entities, EF Core DbContext, schlanke Services
└── Doko.Api              Registriert beide Welten

Frontend
├── GamingLayout          /lobby + /game/:id  →  landscape, kein Nav
└── AppLayout             / + /rules + /analog/*  →  portrait, Nav
```

---

## Bewusst zurückgestellt

- Stats-Seite (komplexe Statistiken, Charts)
- Frontend für Spieler aktiv/inaktiv schalten (Backend-API wird gebaut, Frontend kommt mit Stats)
- Spieler-Accounts (digitale + analoge Spiele verknüpfen)
- ELT-Schicht für kombinierte Statistiken
