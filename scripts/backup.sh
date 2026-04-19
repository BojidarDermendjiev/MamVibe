#!/bin/bash
# MamVibe — PostgreSQL backup script
# Designed to run daily from cron. Keeps the last 7 days of backups.
# Install with: ./scripts/setup-cron.sh

set -euo pipefail

PROJECT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
BACKUP_DIR="$PROJECT_DIR/backups"
TIMESTAMP=$(date +%Y-%m-%d_%H-%M-%S)
BACKUP_FILE="$BACKUP_DIR/mamvibe-$TIMESTAMP.sql.gz"

# Load env vars
set -a
source "$PROJECT_DIR/.env"
set +a

mkdir -p "$BACKUP_DIR"

echo "[$(date)] Starting backup → $BACKUP_FILE"

# Dump from the running postgres container and gzip on the fly
# -T: non-interactive (required for cron)
docker compose -f "$PROJECT_DIR/docker-compose.yml" \
               -f "$PROJECT_DIR/docker-compose.prod.yml" \
               --project-directory "$PROJECT_DIR" \
               exec -T postgres \
               pg_dump -U "$POSTGRES_USER" "$POSTGRES_DB" \
  | gzip > "$BACKUP_FILE"

SIZE=$(du -sh "$BACKUP_FILE" | cut -f1)
echo "[$(date)] Backup complete — $SIZE written to $BACKUP_FILE"

# Remove backups older than 7 days
find "$BACKUP_DIR" -name "mamvibe-*.sql.gz" -mtime +7 -delete
echo "[$(date)] Cleanup done — removed backups older than 7 days"
