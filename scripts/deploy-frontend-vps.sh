#!/bin/bash
# Deploy MIMM Frontend to VPS using password authentication

set -e

# Configuration
VPS_HOST="188.245.68.164"
VPS_PORT="2222"
VPS_USER="mimm"
PASSWORD="MIMMpassword"
WEB_ROOT="/var/www/mimm-frontend"
LOCAL_FILE="/mnt/j/GIT/MIMM-2.0/src/MIMM.Frontend/wwwroot"

echo "🚀 MIMM Frontend Deployment to VPS"
echo "===================================="

# Check if sshpass is installed
if ! command -v sshpass &> /dev/null; then
    echo "⚠️ sshpass not found. Installing..."
    sudo apt-get update
    sudo apt-get install -y sshpass
fi

echo ""
echo "📤 Step 1: Uploading files to VPS..."
echo "   Source: $LOCAL_FILE"
echo "   Target: ${VPS_USER}@${VPS_HOST}:${WEB_ROOT}"

sshpass -p "$PASSWORD" scp -r -P "$VPS_PORT" "$LOCAL_FILE"/* "${VPS_USER}@${VPS_HOST}:${WEB_ROOT}/"

if [ $? -eq 0 ]; then
    echo "✅ Upload successful!"
else
    echo "❌ Upload failed!"
    exit 1
fi

echo ""
echo "🔍 Step 2: Verifying deployment..."

sshpass -p "$PASSWORD" ssh -p "$VPS_PORT" "${VPS_USER}@${VPS_HOST}" "ls -lh ${WEB_ROOT}/js/mood-selector.js && echo '✅ mood-selector.js deployed successfully'"

echo ""
echo "✅ Deployment completed!"
echo "🌐 Test at: https://musicinmymind.app"
echo "📝 Browser: Hard refresh (Ctrl+Shift+R) to load new files"
