# Analytics View Changes

Analytics views live in two places that must always stay in sync:

| Layer | File | Purpose |
|---|---|---|
| dbt model | `Code/database/models/marts/**/*.sql` | Source of truth for the view logic |
| EF migration | `Doko.Analog/Migrations/<timestamp>_<Name>.cs` | Applies the same SQL so `MigrateAsync()` keeps local/server DB in sync |

Both must be updated whenever a view is added or changed.

## How to make a change

### 1. Update the dbt model
Edit the relevant `.sql` file under `Code/database/models/`. Use refs to intermediate models — e.g. `{{ ref('int_team_round_results') }}` — rather than raw table names.

### 2. Create an EF migration
```bash
cd Code/backend
dotnet ef migrations add <DescriptiveName> --project Doko.Analog --startup-project Doko.Api
```

Fill in the generated `Up()` with `migrationBuilder.Sql("""...""")` using `CREATE OR REPLACE VIEW`. The SQL must match the dbt model exactly (translated from dbt refs to real schema-qualified names, e.g. `analog.team_member`).

Also write a `Down()` that restores the previous version of the view.

### 3. Update the entity and DbContext mapping
If the view gains new columns, add them to the entity class (`Stats/<Entity>.cs`) and register the column name in `AnalogDbContext.cs` with `.HasColumnName("column_name")`.

### 4. Update the DTO and controller
Add the new field to the corresponding DTO record in `Doko.Api/DTOs/Analog/StatsDtos.cs` and pass it through in the controller.

## Applying changes locally

**Restart the local backend.** `Program.cs` calls `db.Database.MigrateAsync()` on startup, which automatically applies any pending migrations. No manual `dbt run` needed locally.

## Applying changes on staging / production

Push to `develop` (staging) or `main` (production). The CI/CD pipeline:
1. Rebuilds and restarts the backend container → `MigrateAsync()` runs the EF migration
2. Runs `dbt run --target staging/prod` → recreates all views from the dbt models

Both steps run on every deploy, so the view ends up consistent regardless of which one you consider authoritative.

## Key intermediate models

| Model | What it provides |
|---|---|
| `int_team_round_results` | Per-team per-round: `won`, `team_size`, `solo_multiplier`, `game_value`, `points_won_lost` |
| `int_player_round_results` | Per-player per-round: same fields plus `is_alone`, `is_solo`, `points_earned` |
