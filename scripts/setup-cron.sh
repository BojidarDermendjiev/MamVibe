#!/bin/bash
# MamVibe — One-time cron setup script
# Run this once on your VPS to install automated daily backups.
# Usage: ./scripts/setup-cron.sh

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"
BACKUP_SCRIPT="$SCRIPT_DIR/backup.sh"
LOG_FILE="$PROJECT_DIR/logs/backup.log"

# Make all scripts executable
chmod +x "$SCRIPT_DIR/backup.sh"
chmod +x "$SCRIPT_DIR/restore.sh"
chmod +x "$SCRIPT_DIR/deploy.sh"
echo "Scripts marked executable."

# Create log directory and file (no root required)
mkdir -p "$(dirname "$LOG_FILE")"
touch "$LOG_FILE"
echo "Log file: $LOG_FILE"

# Install daily backup cron at 3:00 AM
CRON_JOB="0 3 * * * $BACKUP_SCRIPT >> $LOG_FILE 2>&1"

if crontab -l 2>/dev/null | grep -qF "$BACKUP_SCRIPT"; then
  echo "Cron job already installed — no changes made."
else
  (crontab -l 2>/dev/null; echo "$CRON_JOB") | crontab -
  echo "Cron job installed: daily backup at 03:00 AM"
fi

echo ""
echo "Current crontab:"
crontab -l

echo ""
echo "Done. To run a backup manually now: $BACKUP_SCRIPT"
echo "To view backup logs: tail -f $LOG_FILE"
