#!/bin/bash
set -e

cd ~/mimm-app

echo "=== 1. Pulling latest code ==="
git pull origin main

echo ""
echo "=== 2. Stopping containers ==="
docker compose -f docker-compose.prod.yml down

echo ""
echo "=== 3. Building backend with .NET 10.0 (no cache) ==="
docker build --no-cache --build-arg CACHEBUST=$(date +%s) -t mimm-backend:latest .

echo ""
echo "=== 4. Starting containers ==="
docker compose -f docker-compose.prod.yml up -d

echo ""
echo "=== 5. Waiting 30s for startup ==="
sleep 30

echo ""
echo "=== 6. Checking backend logs ==="
docker logs mimm-backend --tail 20

echo ""
echo "=== 7. Testing /health endpoint ==="
curl -s http://127.0.0.1:8080/health | head -5 || echo "Health check failed - backend may still be starting"

echo ""
echo "✅ Update complete!"
