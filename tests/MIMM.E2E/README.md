# MIMM E2E Tests (Playwright)

## Prerequisites

- Backend running at <http://localhost:5001>
- Frontend running at <http://localhost:5000>
- Node.js 18+

## Install & Run

```bash
cd tests/MIMM.E2E
npm install
npx playwright install --with-deps
npm test
```

## Config

- Base URLs can be overridden via env:
  - `FRONTEND_URL` (default: <http://localhost:5000>)
  - `BACKEND_URL` (default: <http://localhost:5001>)
  - `TEST_EMAIL` (default: <e2e-auto@example.com>)
  - `TEST_PASSWORD` (default: Test123!)

## Notes

- First test logs in and verifies dashboard, then creates an entry via API and checks it appears in the list (stable smoke test).
- Add further UI interactions next (create via dialog, edit, delete).
