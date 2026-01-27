#!/bin/bash
# VPS Backend Fix Script - Spustit na VPS bez hesla interakce
# Usage: ./vps-fix-backend.sh

set -e

cd ~/mimm-app

echo "=== 1. Pulling latest code from GitHub ==="
git pull origin main

echo ""
echo "=== 2. Stopping containers ==="
docker compose -f docker-compose.prod.yml down

echo ""
echo "=== 3. Removing old backend image ==="
docker rmi mimm-backend:latest || true

echo ""
echo "=== 4. Building new backend image with cache-bust ==="
docker build --build-arg CACHEBUST=$(date +%s) -t mimm-backend:latest .

echo ""
echo "=== 5. Starting containers ==="
docker compose -f docker-compose.prod.yml up -d

echo ""
echo "=== 6. Waiting for backend initialization (30s) ==="
sleep 30

echo ""
echo "=== 7. Checking backend status ==="
docker logs mimm-backend --tail 20

echo ""
echo "=== 8. Testing registration endpoint ==="
RESPONSE=$(curl -s -X POST http://127.0.0.1:8080/api/auth/register \
  -H "Content-Type: application/json" \
  -d "{\"email\":\"test$(date +%s)@example.com\",\"password\":\"Test1234!\",\"displayName\":\"Test User\",\"language\":\"en\"}")

echo "Response:"
echo "$RESPONSE"

echo ""
if echo "$RESPONSE" | grep -q "accessToken"; then
  echo "✅ SUCCESS! Backend returns AuthenticationResponse with accessToken"
else
  echo "❌ FAILED! Backend still returns old structure"
fi
