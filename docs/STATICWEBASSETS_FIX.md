# StaticWebAssets Error Fix - MIMM 2.0

## Problem

When running `dotnet publish` on VPS, you see this error:

```
error : InvalidOperationException: Sequence contains more than one element
   at Microsoft.AspNetCore.StaticWebAssets.Tasks.GenerateStaticWebAssetsDevelopmentManifest.ComputeManifestAssets()
```

## Root Cause

This error occurs when:
1. **Stale cache files** in `obj/staticwebassets/` from previous builds
2. **Duplicate manifest files** after switching .NET versions or build configurations
3. **Inconsistent state** between `bin/`, `obj/`, and NuGet cache

## Solution (3 Steps)

### Step 1: Deep Clean (On VPS)

```bash
cd ~/mimm-app
bash scripts/deep-clean-vps.sh
```

This script:
- Stops all dotnet processes
- Removes ALL `bin/` and `obj/` directories
- Clears NuGet caches (`dotnet nuget locals all -c`)
- Verifies clean state

### Step 2: Fresh Build (On VPS)

```bash
cd ~/mimm-app
bash scripts/update-vps-frontend.sh
```

The updated script now:
- Deep cleans build artifacts first
- Removes `obj/staticwebassets` specifically
- Forces fresh restore (`--force`)
- Publishes with `--no-restore` (uses clean cache)

### Step 3: Verify

```bash
# Check publish output exists
ls -lah ~/mimm-app/publish/frontend/wwwroot/

# Test frontend
curl -I https://musicinmymind.app/login
```

Expected:
- `wwwroot/` folder with `index.html`, `_framework/`, etc.
- HTTP 200 OK response from frontend

---

## Manual Troubleshooting (If Scripts Fail)

### Option A: Manual Deep Clean

```bash
cd ~/mimm-app

# Stop dotnet
pkill -f "dotnet" || true

# Remove all build artifacts
rm -rf src/MIMM.Frontend/bin src/MIMM.Frontend/obj
rm -rf src/MIMM.Shared/bin src/MIMM.Shared/obj

# Clean NuGet
dotnet nuget locals all -c

# Fresh restore
dotnet restore src/MIMM.Frontend/MIMM.Frontend.csproj --force

# Publish (no restore)
dotnet publish src/MIMM.Frontend/MIMM.Frontend.csproj \
  -c Release \
  -o ~/mimm-app/publish/frontend \
  --no-restore
```

### Option B: Nuclear Clean (If Option A Fails)

```bash
cd ~/mimm-app

# Stop everything
docker-compose down || true
pkill -f "dotnet" || true

# Remove EVERYTHING
find . -type d -name "bin" -exec rm -rf {} + 2>/dev/null || true
find . -type d -name "obj" -exec rm -rf {} + 2>/dev/null || true

# Clean ALL NuGet caches
dotnet nuget locals all -c
rm -rf ~/.nuget/packages/microsoft.aspnetcore.components.webassembly*
rm -rf ~/.nuget/packages/microsoft.net.sdk.staticwebassets*

# Pull latest code (in case something was corrupted)
git fetch origin
git reset --hard origin/main

# Full rebuild
dotnet restore MIMM.sln --force
dotnet build MIMM.sln -c Release
dotnet publish src/MIMM.Frontend/MIMM.Frontend.csproj -c Release -o ~/publish
```

---

## Why This Happens

### .NET StaticWebAssets Cache Behavior

Blazor WASM uses **StaticWebAssets** to manage:
- `wwwroot/` files
- NuGet package static files (MudBlazor CSS, icons, etc.)
- Service worker files
- Compressed assets (.br, .gz)

The task `GenerateStaticWebAssetsDevelopmentManifest` creates manifest files in:
- `obj/staticwebassets/msbuild.*.json`
- `obj/staticwebassets.pack.json`
- `obj/staticwebassets.build.json`

**Problem:** If you:
1. Switch .NET versions (e.g., 10.0 → 9.0)
2. Change build configuration (Debug → Release)
3. Update NuGet packages
4. Interrupt a build mid-process

...the manifest files become inconsistent, causing the "Sequence contains more than one element" error.

### Why `dotnet clean` Isn't Enough

`dotnet clean` only removes output directories (`bin/`), but leaves:
- `obj/staticwebassets/` intact
- NuGet cache intact
- Intermediate files intact

**Solution:** Manual `rm -rf obj/` + `dotnet nuget locals all -c`

---

## Prevention

### Best Practices

1. **Always use deep clean scripts** (not `dotnet clean`)
2. **After .NET version changes**, run `deep-clean-vps.sh`
3. **After NuGet package updates**, clear cache
4. **Before deploying**, verify local build works:
   ```bash
   # On your local machine
   rm -rf src/MIMM.Frontend/bin src/MIMM.Frontend/obj
   dotnet nuget locals all -c
   dotnet publish src/MIMM.Frontend/MIMM.Frontend.csproj -c Release
   ```

### Updated Deployment Workflow

```bash
# 1. Deep clean first (if previous builds failed)
bash scripts/deep-clean-vps.sh

# 2. Deploy frontend (script now includes deep clean)
bash scripts/update-vps-frontend.sh

# 3. Verify
curl -I https://musicinmymind.app/login
```

---

## Common Errors & Solutions

| Error | Solution |
|-------|----------|
| `Sequence contains more than one element` | Run `deep-clean-vps.sh` |
| `Could not find a part of the path` | Remove `obj/` directories manually |
| `NuGet restore failed` | `dotnet nuget locals all -c` |
| `NETSDK1045: Cannot find .NET SDK` | Verify `dotnet --version` shows 9.0.x |
| `The process cannot access the file` | Stop all `dotnet` processes |

---

## Verification Commands

```bash
# Check .NET version
dotnet --version  # Should show 9.0.xxx

# Verify no stale processes
ps aux | grep dotnet

# Check for stale build artifacts
find ~/mimm-app -type d -name "obj" | grep staticwebassets

# Verify NuGet cache
ls ~/.nuget/packages/microsoft.aspnetcore.components.webassembly/

# Test build locally before VPS
dotnet publish src/MIMM.Frontend/MIMM.Frontend.csproj -c Release -o /tmp/test-publish
```

---

## Summary

**Root Cause:** Stale StaticWebAssets manifest files in `obj/`

**Quick Fix:**
```bash
cd ~/mimm-app
bash scripts/deep-clean-vps.sh
bash scripts/update-vps-frontend.sh
```

**Prevention:** Always deep clean after .NET version changes or failed builds.

**Fallback:** If scripts fail, use manual nuclear clean (Option B above).
