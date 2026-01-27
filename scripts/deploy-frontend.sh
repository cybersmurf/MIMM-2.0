#!/bin/bash
# Deploy frontend from GitHub artifacts to VPS

set -e

REPO="cybersmurf/MIMM-2.0"
APP_DIR="/home/mimm/mimm-app"
WWWROOT="${APP_DIR}/src/MIMM.Frontend/wwwroot"
DOWNLOAD_DIR="/tmp/frontend-deploy"

echo "🚀 Deploying frontend from GitHub..."

# Create temp directory
mkdir -p "$DOWNLOAD_DIR"
cd "$DOWNLOAD_DIR"

# Download latest artifact from GitHub Actions
echo "📥 Downloading latest frontend build..."
gh run list --repo "$REPO" --workflow=publish-frontend.yml --limit=1 --json databaseId -q '.[0].databaseId' > /tmp/run_id.txt
RUN_ID=$(cat /tmp/run_id.txt)

if [ -z "$RUN_ID" ]; then
  echo "❌ Could not find recent workflow run"
  exit 1
fi

# Download artifact
gh run download "$RUN_ID" --repo "$REPO" --name frontend-wwwroot --dir "$DOWNLOAD_DIR"

# Backup current version
if [ -d "$WWWROOT" ]; then
  echo "📦 Backing up current frontend..."
  tar czf "${WWWROOT}.backup.tar.gz" "$WWWROOT" || true
fi

# Deploy new version
echo "📁 Deploying frontend files..."
cp -r "$DOWNLOAD_DIR"/* "$WWWROOT/"

# Reload Nginx
echo "🔄 Reloading Nginx..."
sudo systemctl reload nginx

# Cleanup
rm -rf "$DOWNLOAD_DIR"

echo "✅ Frontend deployed successfully!"
echo "   Access: https://musicinmymind.app"
