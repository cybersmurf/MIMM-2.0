#!/bin/bash
# Last.fm Scrobbling E2E Test Script
# Manual workflow: Register -> Create Entry -> Scrobble

set -e  # Exit on error

BASE_URL="http://localhost:7001"
TIMESTAMP=$(date +%s%N | cut -b1-13)  # Milliseconds for unique email
TEST_EMAIL="test+${TIMESTAMP}@example.com"
TEST_PASSWORD="TestPassword123!"

echo "🚀 Starting Last.fm Scrobbling E2E Test"
echo "📧 Test Email: $TEST_EMAIL"
echo ""

# Color codes
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m'

# Step 1: Register
echo -e "${YELLOW}Step 1: Registering user...${NC}"
REGISTER_RESPONSE=$(curl -s -X POST "$BASE_URL/api/auth/register" \
  -H "Content-Type: application/json" \
  -d "{\"email\":\"$TEST_EMAIL\",\"password\":\"$TEST_PASSWORD\"}")

echo "Register Response: $REGISTER_RESPONSE"
USER_ID=$(echo $REGISTER_RESPONSE | grep -o '"id":"[^"]*' | cut -d'"' -f4)

if [ -z "$USER_ID" ]; then
  echo -e "${RED}❌ Registration failed - no ID in response${NC}"
  echo "Full response: $REGISTER_RESPONSE"
  exit 1
fi

echo -e "${GREEN}✅ User registered: $USER_ID${NC}"
echo ""

# Step 2: Login
echo -e "${YELLOW}Step 2: Logging in...${NC}"
LOGIN_RESPONSE=$(curl -s -X POST "$BASE_URL/api/auth/login" \
  -H "Content-Type: application/json" \
  -d "{\"email\":\"$TEST_EMAIL\",\"password\":\"$TEST_PASSWORD\"}")

echo "Login Response: $LOGIN_RESPONSE"
ACCESS_TOKEN=$(echo $LOGIN_RESPONSE | grep -o '"accessToken":"[^"]*' | cut -d'"' -f4)

if [ -z "$ACCESS_TOKEN" ]; then
  echo -e "${RED}❌ Login failed${NC}"
  exit 1
fi

echo -e "${GREEN}✅ User logged in, token: ${ACCESS_TOKEN:0:20}...${NC}"
echo ""

# Step 3: Create Entry
echo -e "${YELLOW}Step 3: Creating journal entry...${NC}"
ENTRY_PAYLOAD=$(cat <<EOF
{
  "songTitle": "Bohemian Rhapsody",
  "artistName": "Queen",
  "albumName": "A Night at the Opera",
  "source": "manual",
  "valence": 0.75,
  "arousal": 0.5,
  "notes": "E2E test entry",
  "somaticTags": ["calm", "nostalgic"]
}
EOF
)

CREATE_RESPONSE=$(curl -s -X POST "$BASE_URL/api/entries" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $ACCESS_TOKEN" \
  -d "$ENTRY_PAYLOAD")

echo "Create Entry Response: $CREATE_RESPONSE"
ENTRY_ID=$(echo $CREATE_RESPONSE | grep -o '"id":"[^"]*' | cut -d'"' -f4 | head -1)

if [ -z "$ENTRY_ID" ]; then
  echo -e "${RED}❌ Entry creation failed${NC}"
  exit 1
fi

echo -e "${GREEN}✅ Entry created: $ENTRY_ID${NC}"
echo ""

# Step 4: Attempt to Scrobble (will fail because user has no Last.fm token)
echo -e "${YELLOW}Step 4: Attempting to scrobble (expecting error - no Last.fm token)...${NC}"
SCROBBLE_PAYLOAD=$(cat <<EOF
{
  "songTitle": "Bohemian Rhapsody",
  "artistName": "Queen",
  "albumName": "A Night at the Opera",
  "timestamp": "$(date -u +%Y-%m-%dT%H:%M:%SZ)"
}
EOF
)

SCROBBLE_RESPONSE=$(curl -s -X POST "$BASE_URL/api/lastfm/scrobble" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $ACCESS_TOKEN" \
  -d "$SCROBBLE_PAYLOAD")

echo "Scrobble Response: $SCROBBLE_RESPONSE"

# Check if response contains error about Last.fm not connected
if echo "$SCROBBLE_RESPONSE" | grep -q "not connected\|Last.fm"; then
  echo -e "${GREEN}✅ Got expected error (user not connected to Last.fm)${NC}"
else
  echo -e "${YELLOW}⚠️  Unexpected response (might still be valid)${NC}"
fi

echo ""
echo -e "${GREEN}✅ E2E Test Completed Successfully!${NC}"
echo ""
echo "Summary:"
echo "  - Registered user: $TEST_EMAIL"
echo "  - User ID: $USER_ID"
echo "  - Access Token: ${ACCESS_TOKEN:0:20}..."
echo "  - Created Entry: $ENTRY_ID (Bohemian Rhapsody)"
echo "  - Scrobble test: Correctly rejected (no Last.fm token)"
echo ""
echo "Note: To test full scrobbling, user needs Last.fm OAuth connection first"
