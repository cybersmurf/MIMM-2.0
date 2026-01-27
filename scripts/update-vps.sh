#!/bin/bash
set -e

cd ~/mimm-app

echo "=== MIMM 2.0 VPS Update Script ==="
echo ""

echo "=== 1. Stashing local changes ==="
git stash push -m "Auto-stash before VPS update" || echo "No local changes to stash"

echo ""
echo "=== 2. Pulling latest code from GitHub ==="
git pull origin main

echo ""
echo "=== 3. Stopping containers ==="
docker compose -f docker-compose.prod.yml down

echo ""
echo "=== 4. Building backend with .NET 10.0 (no cache) ==="
docker build --no-cache --build-arg CACHEBUST=$(date +%s) -t mimm-backend:latest .

echo ""
echo "=== 5. Starting containers ==="
docker compose -f docker-compose.prod.yml up -d

echo ""
echo "=== 6. Waiting 30s for startup ==="
sleep 30

echo ""
echo "=== 7. Checking backend logs ==="
docker logs mimm-backend --tail 20

echo ""
echo "=== 8. Testing /health endpoint ==="
curl -s http://127.0.0.1:8080/health | head -5 || echo "⚠️  Health check failed - backend may still be starting"

echo ""
echo "✅ Update complete!"
echo ""
echo "Next steps:"
echo "  - Frontend: Check https://musicinmymind.app/login"
echo "  - API Health: curl -s https://api.musicinmymind.app/health"
echo "  - Backend logs: docker logs mimm-backend -f"
