# Prompt: EF Migrations Review

Goal: Ensure EF Core migrations are consistent, minimal, and safe to apply.

Checklist:

- Verify `ApplicationDbContext` model matches latest migrations (no pending changes).
- Check for accidental drops/renames; ensure explicit column types and constraints.
- Seed data: confirm idempotency.
- Concurrency tokens and required fields preserved.
- Indexes: ensure added/removed indexes are intentional.
- Down methods: present and symmetric.
- Naming: migration names descriptive.

Actions:

1. Run `dotnet ef migrations list` and confirm latest migration is applied in repo.
2. Inspect migration files for unexpected drops/renames.
3. If model changed, generate new migration via `./scripts/ef-add-migration.sh <Name>` and update DB.
4. Summarize findings and recommend apply/no-op.
