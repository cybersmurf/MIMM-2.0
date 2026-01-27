#!/bin/bash
# Installation: sudo cp auto-deploy.sh /usr/local/bin/
# Usage: /usr/local/bin/auto-deploy.sh
# Cron: 0 */6 * * * /usr/local/bin/auto-deploy.sh >> /var/log/mimm-auto-deploy.log 2>&1

set -e

APP_DIR="/home/mimm/mimm-app"
LOG_FILE="/var/log/mimm-auto-deploy.log"
BRANCH="main"
USER="mimm"

log() {
    echo "[$(date '+%Y-%m-%d %H:%M:%S')] $1" | tee -a "$LOG_FILE"
}

# Check if running as root for ownership
if [ "$EUID" -ne 0 ]; then 
    echo "⚠️  Not running as root. Skipping ownership changes."
    SUDO=""
else
    SUDO="sudo -u $USER"
fi

log "=== MIMM 2.0 Auto-Deployment Started ==="

# Check git repo
if [ ! -d "$APP_DIR/.git" ]; then
    log "❌ Git repository not found at $APP_DIR"
    exit 1
fi

cd "$APP_DIR"

# Fetch latest
log "📥 Fetching latest code from GitHub..."
$SUDO git fetch origin "$BRANCH"

# Check if there are updates
BEHIND_COUNT=$($SUDO git rev-list --count HEAD..origin/$BRANCH)
if [ "$BEHIND_COUNT" -eq 0 ]; then
    log "✅ Already up to date"
    exit 0
fi

log "🔄 Found $BEHIND_COUNT new commit(s). Deploying..."

# Pull latest
$SUDO git reset --hard origin/$BRANCH

# Build and restart
log "🐳 Building Docker image..."
docker compose -f docker-compose.prod.yml build --no-cache backend 2>&1 | tee -a "$LOG_FILE"

log "🚀 Restarting services..."
docker compose -f docker-compose.prod.yml up -d 2>&1 | tee -a "$LOG_FILE"

# Wait for health check
log "⏳ Waiting for backend to be healthy..."
for i in {1..30}; do
    if curl -s http://localhost:8080/health > /dev/null 2>&1; then
        log "✅ Backend is healthy"
        break
    fi
    sleep 1
done

log "📊 Service status:"
docker compose -f docker-compose.prod.yml ps | tee -a "$LOG_FILE"

log "✅ Auto-deployment completed successfully"
