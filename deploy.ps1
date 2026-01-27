#!/usr/bin/env pwsh

<#
.SYNOPSIS
Deploy MIMM 2.0 to production VPS

.DESCRIPTION
Automates deployment of backend API and frontend WASM to musicinmymind.app

.PARAMETER Action
Action to perform: 'test', 'build', 'push', 'deploy'

.EXAMPLE
./deploy.ps1 -Action build
./deploy.ps1 -Action push
./deploy.ps1 -Action deploy
#>

param(
    [ValidateSet('test', 'build', 'push', 'deploy')]
    [string]$Action = 'test'
)

$ErrorActionPreference = 'Stop'

# Configuration
$VPS_HOST = '188.245.68.164'
$VPS_PORT = 2222
$VPS_USER = 'mimm'
$VPS_APPDIR = '/home/mimm/mimm-app'
$DOCKER_IMAGE = 'mimm-backend:latest'
$LOCAL_REPO = Get-Location

Write-Host "🚀 MIMM 2.0 Deployment Script" -ForegroundColor Cyan
Write-Host "Action: $Action" -ForegroundColor Green
Write-Host ""

switch ($Action) {
    'test' {
        Write-Host "Testing build..." -ForegroundColor Yellow
        dotnet build src/MIMM.Backend/MIMM.Backend.csproj -c Release -v minimal
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Build test failed"
        }
        Write-Host "✅ Build test passed" -ForegroundColor Green
    }
    
    'build' {
        Write-Host "Building Docker image..." -ForegroundColor Yellow
        $cachebust = (Get-Date -UFormat %s)
        docker build --build-arg CACHEBUST=$cachebust -t $DOCKER_IMAGE .
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Docker build failed"
        }
        Write-Host "✅ Docker image built: $DOCKER_IMAGE" -ForegroundColor Green
    }
    
    'push' {
        Write-Host "Creating deployment archive..." -ForegroundColor Yellow
        
        # Export Docker image
        Write-Host "Exporting Docker image..." -ForegroundColor Cyan
        docker save $DOCKER_IMAGE | wsl gzip > "mimm-backend.tar.gz"
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Docker export failed (requires WSL gzip)"
        }
        $imageSize = (Get-Item "mimm-backend.tar.gz").Length / 1MB
        Write-Host "✅ Image exported: $($imageSize)MB" -ForegroundColor Green
        
        # Transfer to VPS
        Write-Host "Uploading to VPS..." -ForegroundColor Cyan
        Write-Host "Note: This requires SSH key authentication (add to ssh-agent)" -ForegroundColor Yellow
        
        & ssh -p $VPS_PORT "${VPS_USER}@${VPS_HOST}" "mkdir -p ${VPS_APPDIR}/docker-image"
        scp -P $VPS_PORT "mimm-backend.tar.gz" "${VPS_USER}@${VPS_HOST}:${VPS_APPDIR}/docker-image/"
        
        if ($LASTEXITCODE -ne 0) {
            Write-Error "SCP upload failed"
        }
        Write-Host "✅ Uploaded to VPS" -ForegroundColor Green
    }
    
    'deploy' {
        Write-Host "Deploying to VPS..." -ForegroundColor Yellow
        
        # Git pull + rebuild
        $deployCmd = @"
cd ${VPS_APPDIR} && \
echo '📥 Pulling latest code...' && \
git pull origin main && \
echo '🐳 Rebuilding Docker image...' && \
docker compose -f docker-compose.prod.yml build --no-cache backend && \
echo '🚀 Restarting services...' && \
docker compose -f docker-compose.prod.yml up -d && \
echo '✅ Deployment complete'
"@
        
        Write-Host "Executing deployment commands on VPS..." -ForegroundColor Cyan
        & ssh -p $VPS_PORT "${VPS_USER}@${VPS_HOST}" $deployCmd
        
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Deployment failed"
        }
        Write-Host "✅ Deployment successful" -ForegroundColor Green
        
        # Health check
        Write-Host ""
        Write-Host "Checking service health..." -ForegroundColor Cyan
        & ssh -p $VPS_PORT "${VPS_USER}@${VPS_HOST}" "docker compose -f ${VPS_APPDIR}/docker-compose.prod.yml ps"
    }
}

Write-Host ""
Write-Host "Done!" -ForegroundColor Green
