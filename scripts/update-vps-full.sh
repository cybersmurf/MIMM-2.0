#!/bin/bash
set -e

cd ~/mimm-app

echo "=== MIMM 2.0 Full Stack Update (Backend + Frontend) ==="
echo ""

echo "=== Stage 1: Backend Update ==="
bash scripts/update-vps.sh

echo ""
echo "=== Stage 2: Frontend Update ==="
bash scripts/update-vps-frontend.sh

echo ""
echo "✅ Full stack update complete!"
echo ""
echo "Verify:"
echo "  - Backend: curl -s https://api.musicinmymind.app/health | head -3"
echo "  - Frontend: https://musicinmymind.app/login"
