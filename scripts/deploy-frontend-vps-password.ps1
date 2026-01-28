#!/usr/bin/env pwsh
# MIMM Frontend Deployment Script with Password Auth
# Usage: .\scripts\deploy-frontend-vps-password.ps1 -Password "MIMMpassword"

param(
    [string]$Password = "",
    [string]$VpsHost = "188.245.68.164",
    [int]$SshPort = 2222,
    [string]$SshUser = "mimm",
    [string]$WebRoot = "/var/www/mimm-frontend"
)

$ErrorActionPreference = "Stop"

if (-not $Password) {
    Write-Host "❌ Error: Password required!" -ForegroundColor Red
    Write-Host "Usage: .\deploy-frontend-vps-password.ps1 -Password 'your-password'" -ForegroundColor Yellow
    exit 1
}

Write-Host "🚀 MIMM Frontend Deployment to VPS (with Password Auth)" -ForegroundColor Cyan
Write-Host "========================================================" -ForegroundColor Cyan

# Find latest package
$latestPackage = Get-ChildItem frontend-deploy-*.tar.gz -ErrorAction SilentlyContinue | `
    Sort-Object LastWriteTime -Descending | `
    Select-Object -First 1

if (-not $latestPackage) {
    Write-Host "❌ No frontend deployment package found!" -ForegroundColor Red
    exit 1
}

$packageName = $latestPackage.Name
Write-Host "`n📦 Using package: $packageName" -ForegroundColor Cyan
Write-Host "   Size: $('{0:N2}' -f ($latestPackage.Length / 1MB)) MB" -ForegroundColor Gray

# Step 1: Upload
Write-Host "`n📤 Step 1: Uploading to VPS..." -ForegroundColor Yellow

# Create a temp script file for scp/ssh with expect-like behavior
$scpScript = @"
`$password = @'
$Password
'@

# Upload file
`$result = & scp -P $SshPort "$($latestPackage.FullName)" "${SshUser}@${VpsHost}:/home/${SshUser}/"
if (`$LASTEXITCODE -ne 0) {
    Write-Host "❌ Upload failed" -ForegroundColor Red
    exit 1
}
Write-Host "✅ Upload successful" -ForegroundColor Green
"@

# Try using scp with password (will prompt interactively)
Write-Host "Please enter password when prompted:" -ForegroundColor Yellow
$scpResult = & scp -P $SshPort $latestPackage.FullName "${SshUser}@${VpsHost}:/home/${SshUser}/" 2>&1

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ SCP failed!" -ForegroundColor Red
    Write-Host $scpResult -ForegroundColor Red
    exit 1
}

Write-Host "✅ Upload completed" -ForegroundColor Green

# Step 2: Deploy (extract on VPS)
Write-Host "`n🚀 Step 2: Extracting on VPS..." -ForegroundColor Yellow
Write-Host "Please enter password when prompted:" -ForegroundColor Yellow

$deployCommands = @(
    "cd /home/$SshUser && tar -xzf $packageName -C $WebRoot",
    "rm /home/$SshUser/$packageName",
    "echo 'Deployment complete - checking files:' && ls -lh $WebRoot/js/mood-selector.js && ls -lh $WebRoot/index.html"
)

foreach ($cmd in $deployCommands) {
    Write-Host "   Executing: $cmd" -ForegroundColor DarkGray
    ssh -p $SshPort "${SshUser}@${VpsHost}" $cmd
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Deploy failed!" -ForegroundColor Red
        exit 1
    }
}

Write-Host "✅ Deployment completed" -ForegroundColor Green

# Step 3: Verify
Write-Host "`n🔍 Step 3: Verifying..." -ForegroundColor Yellow

try {
    $response = Invoke-WebRequest -Uri "https://musicinmymind.app" -Method Head -UseBasicParsing -TimeoutSec 10
    if ($response.StatusCode -eq 200) {
        Write-Host "✅ Frontend is responding (HTTP 200)" -ForegroundColor Green
    }
} catch {
    Write-Host "⚠️ Could not verify frontend: $($_.Exception.Message)" -ForegroundColor Yellow
}

Write-Host "`n✅ Deployment completed successfully!" -ForegroundColor Green
Write-Host "🌐 Test at: https://musicinmymind.app" -ForegroundColor Cyan
Write-Host "📝 Browser: Hard refresh (Ctrl+Shift+R) to test MoodSelector2D" -ForegroundColor Cyan
