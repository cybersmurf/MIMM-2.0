# Deploy Frontend to VPS
# Usage: .\scripts\deploy-frontend-vps.ps1

param(
    [string]$VpsHost = "188.245.68.164",
    [int]$VpsPort = 2222,
    [string]$VpsUser = "mimm",
    [string]$RemoteWwwroot = "/var/www/mimm-frontend"
)

Write-Host "MIMM Frontend Deployment to VPS" -ForegroundColor Cyan
Write-Host "=================================" -ForegroundColor Gray

# Check if publish directory exists
$publishDir = ".\publish-frontend-new\wwwroot"
if (-not (Test-Path $publishDir)) {
    Write-Host "[ERROR] Publish directory not found: $publishDir" -ForegroundColor Red
    Write-Host "Run: dotnet publish src/MIMM.Frontend/MIMM.Frontend.csproj -c Release -o publish-frontend-new" -ForegroundColor Yellow
    exit 1
}

Write-Host "[OK] Publish directory found" -ForegroundColor Green
Write-Host "Location: $publishDir" -ForegroundColor Gray

# List key files to deploy
Write-Host "`nKey files to deploy:" -ForegroundColor Cyan
$criticalFiles = @(
    "index.html",
    "css/app.css",
    "js/app.js",
    "_framework/blazor.web.js"
)

foreach ($file in $criticalFiles) {
    $fullPath = Join-Path $publishDir $file
    if (Test-Path $fullPath) {
        $size = (Get-Item $fullPath).Length
        Write-Host "  [OK] $file ($($size) bytes)" -ForegroundColor Green
    } else {
        Write-Host "  [SKIP] $file (not found)" -ForegroundColor Yellow
    }
}

Write-Host "`nStarting upload..." -ForegroundColor Cyan
Write-Host "Host: $VpsUser@$VpsHost`:$VpsPort" -ForegroundColor Gray
Write-Host "Target: $RemoteWwwroot" -ForegroundColor Gray

Write-Host "`nYou will be prompted for password (if password auth is enabled)" -ForegroundColor Yellow

# Build SCP command
$scpCmd = "scp -P $VpsPort -r `"$publishDir\*`" `"$VpsUser@$VpsHost`:$RemoteWwwroot/`""
Write-Host "`nExecuting: $scpCmd`n" -ForegroundColor DarkGray

try {
    Invoke-Expression $scpCmd
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "`n[SUCCESS] Upload completed!" -ForegroundColor Green
        Write-Host "`nNext steps:" -ForegroundColor Cyan
        Write-Host "  1. Hard refresh browser (Ctrl+Shift+R)" -ForegroundColor Gray
        Write-Host "  2. Check browser console (F12) for errors" -ForegroundColor Gray
        Write-Host "  3. Verify at https://musicinmymind.app" -ForegroundColor Gray
    } else {
        Write-Host "`n[ERROR] Upload failed with code $LASTEXITCODE" -ForegroundColor Red
        exit 1
    }
}
catch {
    Write-Host "`n[ERROR] $($_)" -ForegroundColor Red
    exit 1
}
