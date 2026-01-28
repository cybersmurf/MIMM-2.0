# MIMM Backend Deployment Script for VPS
# Usage: .\scripts\deploy-backend-vps.ps1

$ErrorActionPreference = "Stop"

Write-Host "=== MIMM Backend Deployment to VPS ===" -ForegroundColor Cyan

# Configuration
$VPS_HOST = "188.245.68.164"
$VPS_PORT = "2222"
$VPS_USER = "mimm"
$BACKEND_PATH = "/home/mimm/mimm-app/src/MIMM.Backend"

# Step 1: Build Backend locally
Write-Host "`n[1/5] Building Backend (Release)..." -ForegroundColor Yellow
Push-Location "src/MIMM.Backend"
dotnet publish -c Release -o ../../publish-backend-new
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    Pop-Location
    exit 1
}
Pop-Location
Write-Host "✅ Build complete" -ForegroundColor Green

# Step 2: Create deployment package
Write-Host "`n[2/5] Creating deployment package..." -ForegroundColor Yellow
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$packageName = "backend-deploy-$timestamp.tar.gz"
Push-Location "publish-backend-new"
tar -czf "../$packageName" .
Pop-Location
Write-Host "✅ Package created: $packageName" -ForegroundColor Green

# Step 3: Upload to VPS
Write-Host "`n[3/5] Uploading to VPS..." -ForegroundColor Yellow
scp -P $VPS_PORT "$packageName" "${VPS_USER}@${VPS_HOST}:/tmp/"
if ($LASTEXITCODE -ne 0) {
    Write-Host "Upload failed!" -ForegroundColor Red
    exit 1
}
Write-Host "✅ Upload complete" -ForegroundColor Green

# Step 4: Deploy on VPS
Write-Host "`n[4/5] Deploying on VPS..." -ForegroundColor Yellow
$deployCommands = @"
set -e
echo '>>> Stopping backend container...'
cd /home/mimm/mimm-app
docker-compose stop backend

echo '>>> Backing up current backend...'
sudo cp -r $BACKEND_PATH ${BACKEND_PATH}.backup-$timestamp

echo '>>> Extracting new backend...'
sudo rm -rf $BACKEND_PATH/*
sudo tar -xzf /tmp/$packageName -C $BACKEND_PATH
sudo chown -R 1000:1000 $BACKEND_PATH

echo '>>> Starting backend container...'
docker-compose up -d backend

echo '>>> Waiting for backend to start...'
sleep 5

echo '>>> Checking backend health...'
docker-compose logs --tail=50 backend

echo '>>> Cleanup...'
rm /tmp/$packageName

echo '✅ Deployment complete!'
"@

ssh -p $VPS_PORT "${VPS_USER}@${VPS_HOST}" $deployCommands
if ($LASTEXITCODE -ne 0) {
    Write-Host "Deployment failed!" -ForegroundColor Red
    exit 1
}
Write-Host "✅ Deployment complete" -ForegroundColor Green

# Step 5: Verify deployment
Write-Host "`n[5/5] Verifying deployment..." -ForegroundColor Yellow
Start-Sleep -Seconds 3
try {
    $healthCheck = Invoke-RestMethod -Uri "https://api.musicinmymind.app/health" -Method Get -TimeoutSec 10
    Write-Host "✅ Backend is healthy: $($healthCheck.status)" -ForegroundColor Green
} catch {
    Write-Host "⚠️ Health check failed: $($_.Exception.Message)" -ForegroundColor Yellow
    Write-Host "Check logs with: ssh -p $VPS_PORT ${VPS_USER}@${VPS_HOST} 'cd mimm-app && docker-compose logs backend'" -ForegroundColor Cyan
}

# Cleanup local package
Write-Host "`nCleaning up local package..." -ForegroundColor Yellow
Remove-Item $packageName -Force
Write-Host "✅ Cleanup complete" -ForegroundColor Green

Write-Host "`n=== Deployment Summary ===" -ForegroundColor Cyan
Write-Host "Backend URL: https://api.musicinmymind.app" -ForegroundColor White
Write-Host "Health check: https://api.musicinmymind.app/health" -ForegroundColor White
Write-Host "Swagger: https://api.musicinmymind.app/swagger" -ForegroundColor White
Write-Host "`nView logs: ssh -p $VPS_PORT ${VPS_USER}@${VPS_HOST} 'cd mimm-app && docker-compose logs -f backend'" -ForegroundColor Cyan
