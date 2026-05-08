#!/usr/bin/env bash
# Push local analog schema data to prod and/or staging PostgreSQL on the server.
#
# Reads POSTGRES_PASSWORD automatically from the server's .env file.
#
# Usage:
#   ./push_to_server.sh <user>@<host> <deploy_path> [prod|staging|both]
#
# Examples:
#   ./push_to_server.sh root@my-server.com /root/opt/doko staging   # staging only
#   ./push_to_server.sh root@my-server.com /root/opt/doko prod      # prod only
#   ./push_to_server.sh root@my-server.com /root/opt/doko           # both (default)

set -euo pipefail

if [[ $# -lt 2 ]]; then
  echo "Usage: $0 <user>@<host> <deploy_path> [prod|staging|both]"
  exit 1
fi

SSH_TARGET="$1"
DEPLOY_PATH="$2"
TARGET="${3:-both}"

if [[ "$TARGET" != "prod" && "$TARGET" != "staging" && "$TARGET" != "both" ]]; then
  echo "Error: target must be prod, staging, or both"
  exit 1
fi

echo "==> Reading POSTGRES_PASSWORD from server..."
PG_PASSWORD=$(ssh "$SSH_TARGET" "grep '^POSTGRES_PASSWORD=' '$DEPLOY_PATH/Code/infrastructure/.env' | cut -d= -f2")
if [[ -z "$PG_PASSWORD" ]]; then
  echo "Error: POSTGRES_PASSWORD not found in $DEPLOY_PATH/Code/infrastructure/.env"
  exit 1
fi

DUMP_FILE="/tmp/analog_data_$(date +%Y%m%d_%H%M%S).sql"

echo "==> Dumping local analog schema..."
PGPASSWORD=postgres pg_dump \
  -h localhost -U postgres doko \
  -n analog \
  --data-only \
  --exclude-table-data=analog.game_mode \
  --exclude-table-data=analog.special_card \
  --exclude-table-data=analog.extra_point \
  -f "$DUMP_FILE"
echo "    Written to $DUMP_FILE"

echo "==> Copying dump to server..."
scp "$DUMP_FILE" "$SSH_TARGET:/tmp/analog_data.sql"

push_to_db() {
  local DB="$1"
  echo "==> Restoring into $DB..."
  ssh "$SSH_TARGET" bash << EOF
set -euo pipefail
docker exec postgres psql -U doko_app "$DB" -c "
  TRUNCATE analog.round_special_card, analog.round_extra_point,
           analog.comment, analog.team_member, analog.team,
           analog.round, analog.player
  RESTART IDENTITY CASCADE;"
docker exec -i postgres psql -U doko_app "$DB" < /tmp/analog_data.sql
echo "  $DB: done"
EOF
}

[[ "$TARGET" == "prod"    || "$TARGET" == "both" ]] && push_to_db doko_analog
[[ "$TARGET" == "staging" || "$TARGET" == "both" ]] && push_to_db doko_analog_staging

echo ""
echo "==> Cleaning up..."
ssh "$SSH_TARGET" "rm /tmp/analog_data.sql"
rm "$DUMP_FILE"

echo "==> All done."
