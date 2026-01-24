# Prompt: Implement Feature (MIMM Pattern)

Input:

- Feature summary (one paragraph)
- Affected layers (Backend/Frontend/Shared)
- API endpoints + DTOs

Plan:

1. Update Shared DTOs (records, required).
2. Backend: Service + Controller + DI wiring.
3. Frontend: Service client + Pages/Components.
4. Tests: Unit + Integration for core flow.
5. Docs: Update README sections.

Rules:

- Use C# 13, net9.0, nullable enabled.
- Keep changes minimal and idiomatic.
- Propagate CancellationToken.
