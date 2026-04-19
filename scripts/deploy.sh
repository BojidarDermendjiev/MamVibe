#!/bin/bash
# MamVibe — Production deploy script
# Usage: ./scripts/deploy.sh
# Pulls latest code, rebuilds images, restarts containers, verifies health.

set -euo pipefail

PROJECT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

echo "[$(date)] === MamVibe deploy starting ==="

# 1. Pull latest code
echo "[$(date)] Pulling latest code..."
git -C "$PROJECT_DIR" pull

# 2. Rebuild and restart containers
echo "[$(date)] Rebuilding and restarting containers..."
docker compose -f "$PROJECT_DIR/docker-compose.yml" \
               -f "$PROJECT_DIR/docker-compose.prod.yml" \
               --project-directory "$PROJECT_DIR" \
               up -d --build

# 3. Wait for the API to come up
echo "[$(date)] Waiting for API to become healthy..."
RETRIES=12
for i in $(seq 1 $RETRIES); do
  if curl -sf http://localhost/health > /dev/null 2>&1; then
    echo "[$(date)] Health check passed on attempt $i."
    break
  fi
  if [[ $i -eq $RETRIES ]]; then
    echo "[$(date)] ERROR: health check failed after $RETRIES attempts."
    echo "Check logs with: docker compose logs -f api"
    exit 1
  fi
  echo "[$(date)] Attempt $i/$RETRIES — retrying in 10s..."
  sleep 10
done

# 4. Show running containers
echo ""
docker compose -f "$PROJECT_DIR/docker-compose.yml" \
               -f "$PROJECT_DIR/docker-compose.prod.yml" \
               --project-directory "$PROJECT_DIR" \
               ps

echo ""
echo "[$(date)] === Deploy complete ==="
