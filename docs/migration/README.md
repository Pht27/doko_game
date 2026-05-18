# Database Migration

## Importing a MySQL backup into local Postgres

The MySQL schema differs significantly from the Postgres `analog` schema — different table names, column names, data types, and the team model (MySQL teams are reusable player-slot objects referenced by `round_has_team`, Postgres teams are round-specific). A direct dump conversion is not possible; the import requires a full ETL.

### One-time manual import

**Prerequisites:** `curl`, `python3`, `psql` (local Postgres running on localhost/doko/postgres/postgres)

1. Download the dump:
   ```bash
   curl -fsSL 'https://raw.githubusercontent.com/Pht27/doko_sql_backup/master/<filename>.sql' \
     > /tmp/mysql_dump.sql
   ```

2. Run the ETL script from this folder:
   ```bash
   python3 etl_mysql_to_postgres.py 2>&1 | \
     PGPASSWORD=postgres psql -h localhost -U postgres -d doko -v ON_ERROR_STOP=1
   ```

3. Verify:
   ```bash
   PGPASSWORD=postgres psql -h localhost -U postgres -d doko -c "
     SELECT COUNT(*) AS players   FROM analog.player;
     SELECT COUNT(*) AS rounds    FROM analog.round;
     SELECT COUNT(*) AS teams     FROM analog.team;
   "
   ```

### MySQL → Postgres table mapping

| MySQL table | Postgres table | Notes |
|---|---|---|
| `player` | `analog.player` | `active`→`IsActive` (bool), `start_points`→`StartingPoints`, `picture_name`→`HeroCard`; `CreatedAt` gets DB default |
| `round` + `round_is_game_mode` | `analog.round` | `winning_party` string→int (`Re`=0, `Kontra`=1); `time_stamp`→`PlayedAt`; `GameModeId` joined from `round_is_game_mode` |
| `team` + `round_has_team` | `analog.team` | MySQL teams are reusable (can appear in many rounds); Postgres teams are per-round. One Postgres team is created per `(round_id, mysql_team_id)` entry in `round_has_team`. New sequential IDs are assigned. |
| `team_has_member` + `round_has_team` | `analog.team_member` | `Position` comes from `round_has_team.position` (seat 1–4) |
| `team_in_round_has_special_card` | `analog.round_special_card` | `team_id` mapped to new Postgres `TeamId` via `(round_id, team_id)` lookup |
| `team_in_round_has_extra_point` | `analog.round_extra_point` | same team mapping |
| `comment` + `round_has_comment` | `analog.comment` | `RoundId` joined from `round_has_comment` |
| `game_mode`, `special_card`, `extra_point` | skipped | Seeded by EF migrations; IDs must match |

---

## Pushing local data to prod / staging

```bash
./push_to_server.sh <user>@<host> <deploy_path> [prod|staging|both]
```

### Examples

```bash
# Both prod and staging (default)
./push_to_server.sh root@178.104.162.39 /root/opt/doko

# Prod only
./push_to_server.sh root@178.104.162.39 /root/opt/doko prod

# Staging only
./push_to_server.sh root@178.104.162.39 /root/opt/doko staging
```

Reads `POSTGRES_PASSWORD` from the server's `.env` over SSH. Dumps the local `analog` schema (data only, excluding lookup tables) and restores into the target database.

---

## Full workflow

```bash
# 1. Download backup
curl -fsSL 'https://raw.githubusercontent.com/Pht27/doko_sql_backup/master/db_backup_YYYY-MM-DD_HH-MM.sql' \
  > /tmp/mysql_dump.sql

# 2. Import into local Postgres
cd docs/migration
python3 etl_mysql_to_postgres.py 2>&1 | \
  PGPASSWORD=postgres psql -h localhost -U postgres -d doko -v ON_ERROR_STOP=1

# 3. Push to server
./push_to_server.sh root@178.104.162.39 /root/opt/doko both
```
