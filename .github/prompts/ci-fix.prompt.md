# Prompt: CI Fix and Optimize

Goal: Diagnose and fix CI failures, optimize performance.

Steps:
1. Inspect workflow logs for failing step.
2. Ensure `dotnet restore/build/test` target `MIMM.sln`.
3. Add OS matrix (`ubuntu/windows/macos`) if needed.
4. Add explicit NuGet cache for `~/.nuget/packages` (Linux/macOS) and Windows path.
5. Ensure coverage upload and artifacts.
6. Propose README/AGENTS updates for clarity.
