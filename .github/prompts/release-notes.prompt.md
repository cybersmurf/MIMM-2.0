# Prompt: Generate Release Notes

Goal: Generate concise release notes for the latest changes.

Context to include:
- Commit history since last tag
- Notable features, fixes, docs, CI changes
- Breaking changes and migration notes

Instructions:
1. Group by type: Features, Fixes, Docs, CI, Refactor, Tests.
2. Use bullet points, keep each item to one line.
3. Include version header (e.g., v1.0.1) and date.
4. Add "Upgrade Notes" when migrations or env changes occur.
