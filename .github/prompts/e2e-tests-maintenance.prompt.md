# Prompt: E2E Tests Maintenance

Goal: Keep integration/E2E tests robust and fast.

Checklist:
- Verify WebApplicationFactory config and test auth handler remain isolated.
- Ensure InMemory EF replaces Npgsql in Testing.
- Add diagnostic output when HTTP status != 2xx.
- Use approximate comparisons for floating points.
- Keep test data deterministic (GUIDs, timestamps).

Actions:
1. Run `dotnet test MIMM.sln -v minimal`.
2. If failures: print response bodies and check middleware.
3. Propose refactor for flaky tests.
