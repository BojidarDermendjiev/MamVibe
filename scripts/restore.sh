#!/bin/bash
# MamVibe — PostgreSQL restore script
# Usage: ./scripts/restore.sh backups/mamvibe-2026-04-15_03-00-00.sql.gz
#
# WARNING: This drops and recreates the database. All current data will be lost.

set -euo pipefail

PROJECT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

if [[ $# -lt 1 ]]; then
  echo "Usage: $0 <backup-file.sql.gz>"
  echo ""
  echo "Available backups:"
  ls -lh "$PROJECT_DIR/backups/"mamvibe-*.sql.gz 2>/dev/null || echo "  (none found in $PROJECT_DIR/backups/)"
  exit 1
fi

BACKUP_FILE="$1"

if [[ ! -f "$BACKUP_FILE" ]]; then
  echo "Error: file not found — $BACKUP_FILE"
  exit 1
fi

# Load env vars
set -a
source "$PROJECT_DIR/.env"
set +a

echo ""
echo "WARNING: This will REPLACE all data in '$POSTGRES_DB' with the backup:"
echo "  $BACKUP_FILE"
echo ""
read -r -p "Type 'yes' to continue: " CONFIRM
if [[ "$CONFIRM" != "yes" ]]; then
  echo "Aborted."
  exit 0
fi

echo "[$(date)] Restoring from $BACKUP_FILE ..."

# Drop and recreate the database, then restore
docker compose -f "$PROJECT_DIR/docker-compose.yml" \
               -f "$PROJECT_DIR/docker-compose.prod.yml" \
               --project-directory "$PROJECT_DIR" \
               exec -T postgres \
               psql -U "$POSTGRES_USER" -c "DROP DATABASE IF EXISTS \"$POSTGRES_DB\";"

docker compose -f "$PROJECT_DIR/docker-compose.yml" \
               -f "$PROJECT_DIR/docker-compose.prod.yml" \
               --project-directory "$PROJECT_DIR" \
               exec -T postgres \
               psql -U "$POSTGRES_USER" -c "CREATE DATABASE \"$POSTGRES_DB\";"

gunzip -c "$BACKUP_FILE" | \
  docker compose -f "$PROJECT_DIR/docker-compose.yml" \
                 -f "$PROJECT_DIR/docker-compose.prod.yml" \
                 --project-directory "$PROJECT_DIR" \
                 exec -T postgres \
                 psql -U "$POSTGRES_USER" "$POSTGRES_DB"

echo "[$(date)] Restore complete."
