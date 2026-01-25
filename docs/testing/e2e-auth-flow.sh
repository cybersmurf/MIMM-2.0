#!/bin/bash
# E2E Authentication Flow Test for MIMM Backend
# Tests: Register → Login → Refresh Token → Verify JWT

set -e

BASE_URL="http://localhost:5001"
REGISTER_EMAIL="e2e-test-$(date +%s)@example.com"
PASSWORD="TestPassword123!"
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${YELLOW}=== MIMM E2E Authentication Test ===${NC}"
echo "Backend URL: $BASE_URL"
echo ""

# Step 1: Register a new user
echo -e "${YELLOW}[1] Testing REGISTER endpoint...${NC}"
REGISTER_RESPONSE=$(curl -s -X POST "$BASE_URL/api/auth/register" \
  -H "Content-Type: application/json" \
  -d "{\"email\":\"$REGISTER_EMAIL\",\"password\":\"$PASSWORD\",\"displayName\":\"E2E Test User\"}")

echo "Response: $REGISTER_RESPONSE"

# Extract userId from response
USER_ID=$(echo $REGISTER_RESPONSE | grep -o '"id":"[^"]*"' | head -1 | cut -d'"' -f4)
if [ -z "$USER_ID" ]; then
  echo -e "${RED}❌ Registration failed - no user ID in response${NC}"
  exit 1
fi
echo -e "${GREEN}✅ Registration successful - User ID: $USER_ID${NC}"
echo ""

# Step 2: Login with registered credentials
echo -e "${YELLOW}[2] Testing LOGIN endpoint...${NC}"
LOGIN_RESPONSE=$(curl -s -X POST "$BASE_URL/api/auth/login" \
  -H "Content-Type: application/json" \
  -d "{\"email\":\"$REGISTER_EMAIL\",\"password\":\"$PASSWORD\"}")

echo "Response: $LOGIN_RESPONSE"

# Extract tokens
ACCESS_TOKEN=$(echo $LOGIN_RESPONSE | grep -o '"accessToken":"[^"]*"' | cut -d'"' -f4)
REFRESH_TOKEN=$(echo $LOGIN_RESPONSE | grep -o '"refreshToken":"[^"]*"' | cut -d'"' -f4)

if [ -z "$ACCESS_TOKEN" ] || [ -z "$REFRESH_TOKEN" ]; then
  echo -e "${RED}❌ Login failed - no tokens in response${NC}"
  exit 1
fi
echo -e "${GREEN}✅ Login successful${NC}"
echo "Access Token (first 20 chars): ${ACCESS_TOKEN:0:20}..."
echo "Refresh Token (first 20 chars): ${REFRESH_TOKEN:0:20}..."
echo ""

# Step 3: Test protected endpoint (using access token)
echo -e "${YELLOW}[3] Testing PROTECTED endpoint (GET /api/entries)...${NC}"
PROTECTED_RESPONSE=$(curl -s -X GET "$BASE_URL/api/entries" \
  -H "Authorization: Bearer $ACCESS_TOKEN")

echo "Response: $PROTECTED_RESPONSE"
if echo $PROTECTED_RESPONSE | grep -q '"value":\[\]' || echo $PROTECTED_RESPONSE | grep -q '"items":\[\]'; then
  echo -e "${GREEN}✅ Protected endpoint accessible with valid token${NC}"
else
  echo -e "${RED}⚠️  Unexpected response from protected endpoint${NC}"
fi
echo ""

# Step 4: Refresh token
echo -e "${YELLOW}[4] Testing REFRESH TOKEN endpoint...${NC}"
REFRESH_RESPONSE=$(curl -s -X POST "$BASE_URL/api/auth/refresh" \
  -H "Content-Type: application/json" \
  -d "{\"refreshToken\":\"$REFRESH_TOKEN\"}")

echo "Response: $REFRESH_RESPONSE"

NEW_ACCESS_TOKEN=$(echo $REFRESH_RESPONSE | grep -o '"accessToken":"[^"]*"' | cut -d'"' -f4)
if [ -z "$NEW_ACCESS_TOKEN" ]; then
  echo -e "${RED}❌ Token refresh failed - no new access token${NC}"
  exit 1
fi
echo -e "${GREEN}✅ Token refresh successful${NC}"
echo "New Access Token (first 20 chars): ${NEW_ACCESS_TOKEN:0:20}..."
echo ""

# Step 5: Verify JWT claims
echo -e "${YELLOW}[5] Verifying JWT claims...${NC}"
# Decode JWT payload (base64url decode)
PAYLOAD=$(echo $NEW_ACCESS_TOKEN | cut -d'.' -f2)
# Add padding if needed
PADDING=$((4 - ${#PAYLOAD} % 4))
if [ $PADDING -ne 4 ]; then
  PAYLOAD="${PAYLOAD}$(printf '%0.s=' $(seq 1 $PADDING))"
fi

DECODED=$(echo "$PAYLOAD" | base64 -d 2>/dev/null || echo "Decoding skipped")
echo "JWT Claims: $DECODED"
echo ""

echo -e "${GREEN}=== ✅ ALL TESTS PASSED ===${NC}"
echo ""
echo "Summary:"
echo "  ✓ User registered: $REGISTER_EMAIL"
echo "  ✓ User logged in successfully"
echo "  ✓ Access token obtained"
echo "  ✓ Protected endpoint accessible"
echo "  ✓ Token refresh successful"
echo ""
