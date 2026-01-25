#!/bin/bash
# Frontend Integration Test Scenario for MIMM
# Tests: Login → Create Entry → Verify in List → Edit → Delete

set -e

FRONTEND_URL="http://localhost:5173"  # Vite dev server (adjust if needed)
API_URL="http://localhost:5001"

GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m'

echo -e "${YELLOW}=== MIMM Frontend Integration Test Plan ===${NC}"
echo ""
echo "Prerequisites:"
echo "1. Backend API running on: $API_URL"
echo "2. Frontend dev server running on: $FRONTEND_URL"
echo "3. PostgreSQL + Redis running (docker-compose up -d)"
echo ""

echo -e "${YELLOW}[PREREQUISITE] Checking backend health...${NC}"
if curl -s "$API_URL/health" | grep -q "healthy"; then
  echo -e "${GREEN}✅ Backend is healthy${NC}"
else
  echo -e "${RED}❌ Backend is not responding. Start it with:${NC}"
  echo "   cd src/MIMM.Backend && dotnet run"
  exit 1
fi
echo ""

echo -e "${YELLOW}[TEST PLAN] Frontend integration scenarios:${NC}"
echo ""
echo "1. Authentication Flow"
echo "   □ Navigate to /login"
echo "   □ Enter credentials from E2E test (e2e-auto@example.com)"
echo "   □ Click Login → verify redirect to /dashboard"
echo "   □ Verify JWT token stored in localStorage"
echo ""

echo "2. Dashboard Initial State"
echo "   □ Verify welcome message displays"
echo "   □ Verify EntryList component renders"
echo "   □ Verify empty state message (no entries yet)"
echo "   □ Verify 'New Entry' button present"
echo ""

echo "3. Create Entry Flow"
echo "   □ Click 'New Entry' → EntryCreateDialog opens"
echo "   □ Enter: Song Title = 'Bohemian Rhapsody'"
echo "   □ Enter: Artist = 'Queen'"
echo "   □ Enter: Album = 'A Night at the Opera'"
echo "   □ Use MoodSelector2D to set:"
echo "     - Valence: +0.7 (positive mood)"
echo "     - Arousal: +0.6 (energetic)"
echo "   □ Set Tension Level: 45"
echo "   □ Select Somatic Tags: [Energetic, Euphoric]"
echo "   □ Enter Notes: 'Epic masterpiece'"
echo "   □ Click Create → verify success notification"
echo "   □ Verify EntryList refreshes with new entry"
echo ""

echo "4. Music Search Integration (Optional)"
echo "   □ In EntryCreateDialog, use MusicSearchBox"
echo "   □ Type '3 body problem' → results load"
echo "   □ Click result → pre-populate form fields"
echo "   □ Verify album art displays"
echo ""

echo "5. Entry List Display"
echo "   □ Verify song title, artist, album display"
echo "   □ Verify mood badge shows correct color"
echo "   □ Verify source badge (manual/itunes/lastfm)"
echo "   □ Verify creation timestamp displays"
echo "   □ Verify Edit/Delete buttons present"
echo "   □ Verify pagination controls visible"
echo ""

echo "6. Edit Entry Flow"
echo "   □ Click Edit on entry → EntryEditDialog opens"
echo "   □ Verify form pre-populated with entry data"
echo "   □ Change: Valence → -0.3 (negative mood)"
echo "   □ Change: Notes → 'Updated comment'"
echo "   □ Click Save → verify success notification"
echo "   □ Verify EntryList updated with new data"
echo ""

echo "7. Delete Entry Flow"
echo "   □ Click Delete on entry → confirm dialog shows"
echo "   □ Click Confirm → entry deleted"
echo "   □ Verify success notification"
echo "   □ Verify entry removed from list"
echo ""

echo "8. Pagination (if multiple entries)"
echo "   □ Create 15+ entries"
echo "   □ Verify pagination controls show"
echo "   □ Test page navigation (Next, Previous, Last)"
echo "   □ Verify page size selector works"
echo ""

echo "9. Form Validation"
echo "   □ Try submit form without Song Title"
echo "   □ Verify error message shows"
echo "   □ Try submit with invalid URL (CoverUrl)"
echo "   □ Verify URL validation error"
echo ""

echo "10. Token Refresh"
echo "   □ Let JWT token expire (15 min timeout in dev)"
echo "   □ Make API call → verify automatic refresh"
echo "   □ Verify new token used for subsequent requests"
echo ""

echo -e "${YELLOW}[MANUAL TEST PROCEDURE]${NC}"
echo ""
echo "Step 1: Start backend"
echo "  $ cd src/MIMM.Backend && dotnet run"
echo ""
echo "Step 2: Start frontend (in new terminal)"
echo "  $ cd src/MIMM.Frontend && npm run dev"
echo ""
echo "Step 3: Open browser"
echo "  $ open http://localhost:5173"
echo ""
echo "Step 4: Execute test scenarios (see above)"
echo ""
echo "Step 5: Verify console logs in browser DevTools"
echo "  - Network tab: check API requests"
echo "  - Application tab: verify localStorage has access_token, refresh_token"
echo "  - Console: check for errors"
echo ""

echo -e "${YELLOW}[AUTOMATED TESTING (When Cypress/Playwright added)]${NC}"
echo ""
echo "// Example Cypress test structure:"
echo 'describe("Entry CRUD Flow", () => {'
echo '  it("should create, edit, and delete an entry", () => {'
echo '    cy.visit("http://localhost:5173/login");'
echo '    cy.get("input[name=email]").type("e2e-auto@example.com");'
echo '    cy.get("input[name=password]").type("Test123!");'
echo '    cy.get("button:contains(Login)").click();'
echo '    cy.url().should("include", "/dashboard");'
echo '    // ... continue with entry creation, verification, cleanup'
echo '  });'
echo '});'
echo ""

echo -e "${GREEN}=== TEST PLAN COMPLETE ===${NC}"
echo ""
echo "For automated testing, install Cypress or Playwright and create"
echo "test files in: tests/MIMM.Tests.E2E/"
echo ""
