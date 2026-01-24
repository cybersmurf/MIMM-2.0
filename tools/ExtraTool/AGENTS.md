# Agents.md

> **Note**: This is an EXAMPLE configuration for an external tool. Adapt this structure for your own tools.
> This folder is a template/placeholder for future external tools.

## Example Structure for External Tools

When adding a new tool to this folder, create:

- `src/` - Source code folder
- `YourTool.csproj` - Project file
- `AGENTS.md` - Tool-specific instructions

## Example Prerequisites

- Verify .NET 9.0 SDK is installed: `dotnet --info`
- Install required packages: `dotnet restore`

## Example Build Commands

```bash
dotnet restore
dotnet build
dotnet run
```

- Run: `dotnet run --project src/Demolice.csproj` (from repository root) or `dotnet run` (from `src` folder)

---

**For main repository documentation, see the root-level AGENTS.md, CLAUDE.md, and GEMINI.md files.**
