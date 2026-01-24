# Prompt: API Contract Review

Goal: Validate API surface and DTO contracts remain stable and documented.

Checklist:

- DTOs: `record`, `required`, nullable correctly; versioned when breaking.
- Controllers/Endpoints: consistent routes, verbs, status codes; `[Authorize]` where needed.
- Shared models: ensure frontend/backed alignment.
- OpenAPI/Swagger: confirm document generation; examples if available.
- Error handling: consistent problem details payloads.

Actions:

1. List public endpoints (Auth, Entries, etc.) and compare with shared DTOs.
2. Flag breaking changes (field removals/renames) and propose versioning/migration notes.
3. Ensure status codes and error shapes are tested (integration tests).
4. Recommend doc updates (README / API docs) for any new/changed endpoints.
