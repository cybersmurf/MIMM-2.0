# MIMM Frontend Deployment with SSH Key and Fallback to Password
# Usage: .\scripts\deploy-frontend-vps-key.ps1 -UsePassword

param(
    [switch]$UsePassword = $false
)

$ErrorActionPreference = "Stop"

Write-Host "🚀 MIMM Frontend Deployment to VPS" -ForegroundColor Cyan
Write-Host "===================================" -ForegroundColor Cyan

$VPS_HOST = "188.245.68.164"
$VPS_PORT = "2222"
$VPS_USER = "mimm"
$WEB_ROOT = "/var/www/mimm-frontend"
$SSH_KEY = "$env:USERPROFILE\.ssh\mimm_vps"
$LOCAL_PATH = "src/MIMM.Frontend/wwwroot"

# Verify local path
if (-not (Test-Path $LOCAL_PATH)) {
    Write-Host "❌ Local path not found: $LOCAL_PATH" -ForegroundColor Red
    exit 1
}

Write-Host "`n✅ Source directory: $LOCAL_PATH" -ForegroundColor Green

# Step 1: Try SSH key first
if (-not $UsePassword -and (Test-Path $SSH_KEY)) {
    Write-Host "`n📤 Step 1: Uploading with SSH key..." -ForegroundColor Yellow
    Write-Host "   Using key: $SSH_KEY" -ForegroundColor Gray
    
    scp -i $SSH_KEY -r -P $VPS_PORT "$LOCAL_PATH\*" "${VPS_USER}@${VPS_HOST}:${WEB_ROOT}/"
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Upload successful with SSH key!" -ForegroundColor Green
        
        # Verify
        Write-Host "`n🔍 Verifying..." -ForegroundColor Yellow
        ssh -i $SSH_KEY -p $VPS_PORT ${VPS_USER}@${VPS_HOST} "ls -lh ${WEB_ROOT}/js/mood-selector.js && echo '✅ Verified!'"
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "`n✅ Deployment completed successfully!" -ForegroundColor Green
            Write-Host "🌐 Test at: https://musicinmymind.app" -ForegroundColor Cyan
            exit 0
        }
    }
    
    Write-Host "⚠️ SSH key auth failed, trying password..." -ForegroundColor Yellow
}

# Step 2: Fallback to password
Write-Host "`n📤 Step 2: Uploading with password..." -ForegroundColor Yellow
Write-Host "   You will be prompted for password." -ForegroundColor Gray
Write-Host "   Enter: MIMMpassword" -ForegroundColor Yellow

scp -r -P $VPS_PORT "$LOCAL_PATH\*" "${VPS_USER}@${VPS_HOST}:${WEB_ROOT}/"

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n✅ Upload successful with password!" -ForegroundColor Green
    
    # Verify
    Write-Host "`n🔍 Verifying..." -ForegroundColor Yellow
    ssh -p $VPS_PORT ${VPS_USER}@${VPS_HOST} "ls -lh ${WEB_ROOT}/js/mood-selector.js && echo '✅ Verified!'"
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "`n✅ Deployment completed successfully!" -ForegroundColor Green
        Write-Host "🌐 Test at: https://musicinmymind.app" -ForegroundColor Cyan
        exit 0
    }
} else {
    Write-Host "`n❌ Upload failed!" -ForegroundColor Red
    exit 1
}
