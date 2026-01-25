#!/usr/bin/env bash
set -euo pipefail

# Cleanup E2E test data from PostgreSQL.
# Targets entries belonging to the E2E test user (default: e2e-auto@example.com).

DB_HOST="${DB_HOST:-127.0.0.1}"
DB_PORT="${DB_PORT:-5432}"
DB_NAME="${DB_NAME:-mimm}"
DB_USER="${DB_USER:-mimmuser}"
DB_PASSWORD="${DB_PASSWORD:-mimmpass}"
E2E_EMAIL="${TEST_EMAIL:-e2e-auto@example.com}"

SQL="DELETE FROM \"Entries\" e USING \"Users\" u WHERE e.\"UserId\" = u.\"Id\" AND u.\"Email\"='${E2E_EMAIL}';"

echo "[cleanup] Deleting E2E entries for user: ${E2E_EMAIL}"

if docker ps --format '{{.Names}}' | grep -q '^mimm-postgres$'; then
  echo "[cleanup] Using docker exec into mimm-postgres"
  docker exec -e PGPASSWORD="${DB_PASSWORD}" mimm-postgres \
    psql -U "${DB_USER}" -d "${DB_NAME}" -c "${SQL}"
else
  echo "[cleanup] Using local psql against ${DB_HOST}:${DB_PORT}/${DB_NAME}"
  export PGPASSWORD="${DB_PASSWORD}"
  psql -h "${DB_HOST}" -p "${DB_PORT}" -U "${DB_USER}" -d "${DB_NAME}" -c "${SQL}"
fi

echo "[cleanup] Done."
