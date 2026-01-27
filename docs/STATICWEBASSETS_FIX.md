# StaticWebAssets Error Fix - MIMM 2.0

## Problem

When running `dotnet publish` on VPS, you see this error:

```
error : InvalidOperationException: Sequence contains more than one element
   at Microsoft.AspNetCore.StaticWebAssets.Tasks.GenerateStaticWebAssetsDevelopmentManifest.ComputeManifestAssets()
```

## Root Cause

**System.Text.Json NuGet Package Conflict**

In .NET 9.0, `System.Text.Json` is part of the runtime and **already provides static web assets**. 

When the code had:
```xml
<PackageReference Include="System.Text.Json" Version="9.0.0" />
```

This created a **duplicate static web asset definition**, causing:
```
InvalidOperationException: Sequence contains more than one element
at GenerateStaticWebAssetsDevelopmentManifest.ComputeManifestAssets()
```

## Why This Happens

Blazor WASM's `GenerateStaticWebAssetsDevelopmentManifest` task:
1. Collects all static assets from:
   - Runtime (includes System.Text.Json)
   - NuGet packages (MudBlazor, Blazored.LocalStorage, etc.)
   - Project files
2. Expects **exactly one entry per asset type**
3. Fails if **System.Text.Json appears twice** (runtime + NuGet package)

## Solution

**Already Fixed** ✅ in commit `ded5bee`

The issue was a configuration problem, not a build infrastructure issue.

**Already Fixed** ✅ in commit `ded5bee`

The issue was a configuration problem, not a build infrastructure issue.

---

## Manual Troubleshooting (If Scripts Fail)

If you still encounter this error despite the fix, it means duplicate NuGet packages are defined elsewhere.

### Check for Duplicates

```bash
grep "System.Text.Json" src/*/MIMM.*.csproj
# Should return NOTHING - if it does, remove the line

grep "System.Net.Http" src/*/MIMM.*.csproj
# System.Net.Http also comes with .NET 9.0 - remove if explicit NuGet
```

### Option A: Manual Fix

Edit `src/MIMM.Frontend/MIMM.Frontend.csproj`:

```xml
<ItemGroup>
    <!-- Remove any of these that appear - they're built into .NET 9.0 -->
    <!-- DELETE these lines if present: -->
    <!-- <PackageReference Include="System.Text.Json" Version="9.0.0" /> -->
    <!-- <PackageReference Include="System.Net.Http.Json" Version="9.0.0" /> -->
    <!-- <PackageReference Include="System.Reflection.*" ... /> -->
    
    <!-- Keep these - they're needed: -->
    <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly" Version="9.0.0" />
    <PackageReference Include="MudBlazor" Version="7.0.0" />
    <PackageReference Include="Blazored.LocalStorage" Version="4.4.0" />
</ItemGroup>
```

Then clean & rebuild:
```bash
rm -rf src/MIMM.Frontend/bin src/MIMM.Frontend/obj
dotnet nuget locals all -c
dotnet restore src/MIMM.Frontend/MIMM.Frontend.csproj
dotnet publish src/MIMM.Frontend/MIMM.Frontend.csproj -c Release
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

---

## Prevention for Future

### .NET 9.0 Built-in Packages

These packages are **already included in .NET 9.0 runtime**. Do NOT add them to MIMM.Frontend.csproj:

```
❌ DO NOT ADD:
- System.Text.Json
- System.Net.Http.Json
- System.Collections
- System.Reflection.*
- System.Linq
- System.IO
- System.Threading.Tasks
```

These **should** be in .csproj:
```
✅ DO ADD:
- Microsoft.AspNetCore.Components.WebAssembly
- Microsoft.AspNetCore.Components.Authorization
- Microsoft.AspNetCore.SignalR.Client
- MudBlazor
- Blazored.LocalStorage
- Refit
```

---

## Summary

**Root Cause:** Explicit `System.Text.Json` NuGet package conflicted with built-in .NET 9.0 version

**Fix:** Removed from [src/MIMM.Frontend/MIMM.Frontend.csproj](../src/MIMM.Frontend/MIMM.Frontend.csproj)

**Status:** ✅ Fixed in commit `ded5bee`
