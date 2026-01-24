# Agents.md

> **Note**: This is an EXAMPLE configuration for an external tool. Adapt this structure for your own tools.

- Code is in `src` folder
- Code is an ASP.NET Core web application (REST API with Swagger/OpenAPI)
- Project uses .NET 10.0 (specified in `src/Demolice.csproj`)

## Prerequisites
- Verify .NET 10.0 SDK is installed: `dotnet --info`
- Required packages: Azure.AI.OpenAI, Microsoft.EntityFrameworkCore.Sqlite

## Setup & build
- Install dependencies: `dotnet restore` (run from `src` folder)
- Rebuild: `dotnet build` (run from `src` folder)
- Run: `dotnet run --project src/Demolice.csproj` (from repository root) or `dotnet run` (from `src` folder)

---

**For main repository documentation, see the root-level AGENTS.md, CLAUDE.md, and GEMINI.md files.**