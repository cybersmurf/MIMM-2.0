#!/bin/bash
# MIMM 2.0 Production Deployment Script
# Run on VPS: bash ~/mimm-app/deploy-production.sh

set -e  # Exit on error

echo "🚀 MIMM 2.0 Production Deployment"
echo "==================================="
echo ""

# Colors
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

APP_DIR="/home/mimm/mimm-app"

# Check if running from correct directory
if [ ! -f "docker-compose.prod.yml" ]; then
    echo -e "${RED}❌ Error: docker-compose.prod.yml not found${NC}"
    echo "Run from: $APP_DIR"
    exit 1
fi

echo -e "${YELLOW}Step 1: Pulling latest code from GitHub...${NC}"
git fetch origin main
git reset --hard origin/main
if [ $? -eq 0 ]; then
    echo -e "${GREEN}✅ Code updated${NC}"
else
    echo -e "${RED}❌ Git pull failed${NC}"
    exit 1
fi

echo ""
echo -e "${YELLOW}Step 2: Rebuilding Docker image (backend)...${NC}"
docker compose -f docker-compose.prod.yml build --no-cache backend
if [ $? -eq 0 ]; then
    echo -e "${GREEN}✅ Docker image built${NC}"
else
    echo -e "${RED}❌ Docker build failed${NC}"
    exit 1
fi

echo ""
echo -e "${YELLOW}Step 3: Restarting services...${NC}"
docker compose -f docker-compose.prod.yml up -d
if [ $? -eq 0 ]; then
    echo -e "${GREEN}✅ Services restarted${NC}"
else
    echo -e "${RED}❌ Service restart failed${NC}"
    exit 1
fi

echo ""
echo -e "${YELLOW}Step 4: Service Status${NC}"
docker compose -f docker-compose.prod.yml ps

echo ""
echo -e "${YELLOW}Step 5: Waiting for backend to be healthy...${NC}"
sleep 3
docker compose -f docker-compose.prod.yml logs backend --tail 20

echo ""
echo -e "${GREEN}✅ Deployment complete!${NC}"
echo ""
echo "Frontend: https://musicinmymind.app"
echo "Backend API: https://api.musicinmymind.app"
echo "Backend Health: http://localhost:8080/health"
echo ""
echo "Monitor logs: docker compose -f docker-compose.prod.yml logs -f backend"
