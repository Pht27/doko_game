# dbt-Infrastruktur für Spielstatistiken

## Kontext & Entscheidung

Analoge Spiele werden in der `analog`-Schema in PostgreSQL gespeichert. Stats werden aktuell
on-demand via LINQ im `AnalogPlayersService` berechnet. Mit wachsender Komplexität
(Spielmodus-Breakdowns, Head-to-Head, Sonderkarten, später digitale Spiele) werden
SQL-Transformationen in dbt übersichtlicher und testbarer als LINQ.

**Entscheidung:** dbt trotz Single-DB — bewusst als Lernprojekt und zur Vorbereitung
auf spätere Komplexität (zweite Datenquelle, viele Unterscheidungen Solo/Team/gemischte Auswertung).

**Dieser Task:** Nur Infrastruktur aufsetzen. Das erste Modell (Leaderboard) kommt im
Folge-Task.

---

## Ziel-Architektur

```
PostgreSQL
├── analog.*          ← Quelldaten (EF Core schreibt hier rein)
└── analytics.*       ← dbt-Output (Views/Materialized Views)

dbt/                  ← neues Verzeichnis im Repo-Root
├── dbt_project.yml
├── profiles.yml       ← gitignored, lokal + per Env-Var auf Server
├── .gitignore
└── models/
    ├── staging/analog/    ← dünn, 1:1-Abbild der Source-Tables
    ├── intermediate/      ← Zwischenschicht (später)
    └── marts/             ← Finale Stats-Views für API (später)
```

---

## 1. Lokale Installation

### Voraussetzungen
- Python 3.9+ (`python --version`)
- Lokale PostgreSQL-Instanz mit DB `doko` (bereits vorhanden)

### dbt installieren

```bash
pip install dbt-postgres
# oder mit uv (empfohlen falls vorhanden):
uv tool install dbt-postgres

dbt --version  # sollte dbt-core + dbt-postgres zeigen
```

### Projekt initialisieren

```bash
# Im Repo-Root:
mkdir dbt && cd dbt
dbt init doko --skip-profile-setup
# Legt dbt_project.yml + models/-Ordner an; profiles.yml separat (s.u.)
```

### profiles.yml anlegen (gitignored!)

Datei: `dbt/profiles.yml`

```yaml
doko:
  target: dev
  outputs:
    dev:
      type: postgres
      host: localhost
      port: 5432
      dbname: doko
      user: postgres
      password: postgres
      schema: analytics
      threads: 1
    prod:
      type: postgres
      host: postgres          # Docker-Service-Name
      port: 5432
      dbname: doko_analog
      user: doko_app
      password: "{{ env_var('POSTGRES_PASSWORD') }}"
      schema: analytics
      threads: 1
    staging:
      type: postgres
      host: postgres
      port: 5432
      dbname: doko_analog_staging
      user: doko_app
      password: "{{ env_var('POSTGRES_PASSWORD') }}"
      schema: analytics
      threads: 1
```

### .gitignore für dbt

Datei: `dbt/.gitignore`

```
profiles.yml
target/
dbt_packages/
logs/
```

### Verbindung testen

```bash
cd dbt
dbt debug  # Prüft Verbindung + Konfiguration
```

---

## 2. dbt_project.yml konfigurieren

Datei: `dbt/dbt_project.yml`

```yaml
name: doko
version: '1.0.0'
config-version: 2
profile: doko

model-paths: ["models"]
test-paths: ["tests"]
seed-paths: ["seeds"]
macro-paths: ["macros"]

target-path: "target"
clean-targets: ["target", "dbt_packages"]

models:
  doko:
    staging:
      +materialized: view
    intermediate:
      +materialized: view
    marts:
      +materialized: view   # später ggf. auf table für Performance
```

---

## 3. Source-Deklaration & Staging-Modelle

Diese Dateien stellen sicher, dass dbt die `analog`-Schema-Tables kennt
und beim späteren Modellbau darauf referenziert werden kann.

### Source-Deklaration

Datei: `dbt/models/staging/analog/_sources.yml`

```yaml
version: 2

sources:
  - name: analog
    schema: analog
    tables:
      - name: player
        description: "Analoge Spieler"
      - name: round
        description: "Einzelne Runden/Spiele"
      - name: game_mode
        description: "Spielmodi (Normal, Solo, Hochzeit, ...)"
      - name: team
        description: "Re/Kontra-Teams pro Runde"
      - name: team_member
        description: "Spieler-zu-Team-Zuordnung"
      - name: special_card
        description: "Sonderkarten-Definitionen"
      - name: round_special_card
        description: "Welche Sonderkarten in welcher Runde"
      - name: extra_point
        description: "Extrapunkt-Definitionen"
      - name: round_extra_point
        description: "Welche Extrapunkte in welcher Runde"
      - name: comment
        description: "Kommentare zu Runden"
```

### Staging-Modelle (thin wrappers)

Diese Modelle tun noch nichts Komplexes — sie benennen Spalten konsistent
und dienen als stabiler Einstiegspunkt für spätere Intermediate-/Mart-Modelle.

`dbt/models/staging/analog/stg_analog__players.sql`:
```sql
select
    id          as player_id,
    name,
    is_active,
    starting_points,
    created_at
from {{ source('analog', 'player') }}
```

`dbt/models/staging/analog/stg_analog__rounds.sql`:
```sql
select
    id              as round_id,
    played_at,
    winning_party,
    points,
    game_mode_id
from {{ source('analog', 'round') }}
```

`dbt/models/staging/analog/stg_analog__game_modes.sql`:
```sql
select
    id          as game_mode_id,
    name,
    is_solo,
    solo_party
from {{ source('analog', 'game_mode') }}
```

`dbt/models/staging/analog/stg_analog__teams.sql`:
```sql
select
    id          as team_id,
    round_id,
    party,
    name        as team_name
from {{ source('analog', 'team') }}
```

`dbt/models/staging/analog/stg_analog__team_members.sql`:
```sql
select
    team_id,
    player_id,
    position
from {{ source('analog', 'team_member') }}
```

`dbt/models/staging/analog/stg_analog__round_extra_points.sql`:
```sql
select
    round_id,
    extra_point_id,
    team_id,
    count
from {{ source('analog', 'round_extra_point') }}
```

`dbt/models/staging/analog/stg_analog__round_special_cards.sql`:
```sql
select
    round_id,
    special_card_id,
    team_id
from {{ source('analog', 'round_special_card') }}
```

### Erster Smoke-Test

```bash
cd dbt
dbt run          # alle Staging-Views in analytics_staging-Schema anlegen
dbt test         # keine Tests definiert, sollte grün durchlaufen
dbt docs generate && dbt docs serve  # optionaler Data-Lineage-Graph
```

---

## 4. Datenbankänderungen

### analytics-Schema für doko_app freigeben

`Code/infrastructure/postgres/init.sql` erweitern:

```sql
-- Bestehend: staging-DB anlegen
CREATE DATABASE doko_analog_staging;
GRANT ALL PRIVILEGES ON DATABASE doko_analog_staging TO doko_app;

-- NEU: analytics-Schema in beiden DBs anlegen und Rechte vergeben
\c doko_analog
CREATE SCHEMA IF NOT EXISTS analytics;
GRANT ALL ON SCHEMA analytics TO doko_app;
ALTER DEFAULT PRIVILEGES IN SCHEMA analytics GRANT ALL ON TABLES TO doko_app;

\c doko_analog_staging
CREATE SCHEMA IF NOT EXISTS analytics;
GRANT ALL ON SCHEMA analytics TO doko_app;
ALTER DEFAULT PRIVILEGES IN SCHEMA analytics GRANT ALL ON TABLES TO doko_app;
```

**Lokal** (einmalig ausführen):
```bash
psql -U postgres -d doko -c "CREATE SCHEMA IF NOT EXISTS analytics;"
```

---

## 5. Docker-Integration

### Neuer dbt-Service in docker-compose.yml

`Code/infrastructure/docker-compose.yml` — neuen Service ergänzen:

```yaml
  dbt:
    build:
      context: ../../dbt
      dockerfile: ../Code/infrastructure/dbt/Dockerfile
    environment:
      - POSTGRES_PASSWORD=${POSTGRES_PASSWORD}
    volumes:
      - ../../dbt:/dbt
    working_dir: /dbt
    command: ["dbt", "run", "--target", "prod"]
    networks:
      - doko-net
    depends_on:
      - postgres
    restart: "no"   # läuft einmalig durch, kein Daemon
    profiles:
      - dbt          # nicht auto-start, nur via: docker compose --profile dbt up dbt
```

### Dockerfile für dbt

Neue Datei: `Code/infrastructure/dbt/Dockerfile`

```dockerfile
FROM python:3.12-slim
RUN pip install --no-cache-dir dbt-postgres==1.9.*
WORKDIR /dbt
COPY . .
```

### dbt manuell auf Server ausführen

```bash
# Prod:
docker compose -f Code/infrastructure/docker-compose.yml \
  --env-file Code/infrastructure/.env \
  --profile dbt \
  run --rm dbt dbt run --target prod

# Staging:
docker compose -f Code/infrastructure/docker-compose.yml \
  --env-file Code/infrastructure/.env \
  --profile dbt \
  run --rm dbt dbt run --target staging
```

---

## 6. Automatisierung auf dem Server

Analog zum bestehenden `staging-db-reset`-Timer wird ein systemd-Timer
für regelmäßige dbt-Runs angelegt. Das ist Infrastruktur-Ansible-Arbeit
und kommt nach dem ersten manuellen Betrieb — hier nur als Konzept dokumentiert:

- **Timer:** täglich nach dem staging-db-reset (z.B. 04:30 UTC)
- **Service:** führt `docker compose --profile dbt run --rm dbt dbt run --target prod` aus,
  danach nochmal mit `--target staging`
- **Platzierung:** `/etc/systemd/system/dbt-run.service` + `.timer` via Ansible `maintenance`-Role

---

## 7. GitHub Actions (optional, für CI-Validierung)

`.github/workflows/dbt-check.yml` — prüft bei jedem PR ob dbt-Modelle kompilieren:

```yaml
name: dbt check

on:
  push:
    branches: [main, develop]
    paths:
      - 'dbt/**'
  pull_request:
    paths:
      - 'dbt/**'

jobs:
  dbt-compile:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-python@v5
        with:
          python-version: '3.12'
      - run: pip install dbt-postgres
      - name: dbt compile (dry-run, kein DB-Zugriff)
        working-directory: dbt
        env:
          DBT_PROFILES_DIR: .
        run: |
          # profiles.yml für CI ohne echte DB:
          cat > profiles.yml << 'EOF'
          doko:
            target: dev
            outputs:
              dev:
                type: postgres
                host: localhost
                port: 5432
                dbname: doko
                user: postgres
                password: postgres
                schema: analytics
                threads: 1
          EOF
          dbt compile  # kompiliert SQL ohne DB-Verbindung
```

---

## 8. Checkliste: Infrastruktur fertig wenn...

- [ ] `dbt/` Verzeichnis im Repo mit `dbt_project.yml`, `.gitignore`, `models/staging/analog/`
- [ ] `dbt/profiles.yml` lokal vorhanden (gitignored), `dbt debug` läuft grün
- [ ] `dbt run` legt alle Staging-Views in lokalem `analytics`-Schema an
- [ ] `analytics`-Schema-Grants in `postgres/init.sql` ergänzt
- [ ] `dbt/Dockerfile` + Service-Eintrag in `docker-compose.yml`
- [ ] `dbt run --target prod` funktioniert via Docker auf dem Server

Danach: Folge-Task "Leaderboard implementieren" — erstes Intermediate- + Mart-Modell,
.NET API liest aus `analytics.player_leaderboard`.
