#!/bin/bash

# MIMM 2.0 - Quick Setup Script
# This script automates the initial setup process

set -e  # Exit on error

echo "🎵 MIMM 2.0 Setup Script"
echo "========================"
echo ""

# Check prerequisites
echo "🔍 Checking prerequisites..."

if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET SDK not found. Please install .NET 9 SDK first."
    echo "   Download: https://dotnet.microsoft.com/download/dotnet/9.0"
    exit 1
fi

if ! command -v docker &> /dev/null; then
    echo "❌ Docker not found. Please install Docker Desktop first."
    echo "   Download: https://www.docker.com/products/docker-desktop"
    exit 1
fi

echo "✅ .NET SDK found: $(dotnet --version)"
echo "✅ Docker found: $(docker --version | cut -d' ' -f3)"
echo ""

# Copy environment template
if [ ! -f .env ]; then
    echo "📋 Creating .env file from template..."
    cp .env.example .env
    echo "✅ .env created. Please edit it with your settings!"
    echo ""
else
    echo "⚠️  .env already exists, skipping..."
    echo ""
fi

# Start Docker services
echo "🐳 Starting Docker services (PostgreSQL + Redis)..."
docker-compose up -d postgres redis
echo "⏳ Waiting for PostgreSQL to be ready..."
sleep 5
echo "✅ Database containers started"
echo ""

# Restore NuGet packages
echo "📦 Restoring NuGet packages..."
dotnet restore
echo "✅ Packages restored"
echo ""

# Build solution
echo "🔨 Building solution..."
dotnet build --no-restore
echo "✅ Build successful"
echo ""

# Install EF Core tools
echo "🔧 Installing Entity Framework Core tools..."
if ! dotnet tool list -g | grep -q "dotnet-ef"; then
    dotnet tool install --global dotnet-ef
    echo "✅ EF Core tools installed"
else
    echo "✅ EF Core tools already installed"
fi
echo ""

# Apply migrations
echo "🗄️  Applying database migrations..."
cd src/MIMM.Backend
dotnet ef database update
cd ../..
echo "✅ Migrations applied"
echo ""

# Generate JWT secret if not set
if grep -q "your-256-bit-secret-key-change-this" .env; then
    echo "🔐 Generating JWT secret..."
    JWT_SECRET=$(openssl rand -base64 32)
    if [[ "$OSTYPE" == "darwin"* ]]; then
        sed -i '' "s/your-256-bit-secret-key-change-this-in-production-at-least-32-characters-long/$JWT_SECRET/" .env
    else
        sed -i "s/your-256-bit-secret-key-change-this-in-production-at-least-32-characters-long/$JWT_SECRET/" .env
    fi
    echo "✅ JWT secret generated"
    echo ""
fi

# Setup complete
echo "✅ Setup Complete!"
echo ""
echo "📝 Next steps:"
echo "   1. Edit .env file with your API keys (Last.fm, SendGrid, etc.)"
echo "   2. Run backend: cd src/MIMM.Backend && dotnet run"
echo "   3. Run frontend: cd src/MIMM.Frontend && dotnet run"
echo "   4. Open browser: https://localhost:5001"
echo ""
echo "📚 Documentation:"
echo "   - Setup Guide: SETUP_GUIDE.md"
echo "   - Migration Guide: MIGRATION_GUIDE.md"
echo "   - Swagger UI: https://localhost:7001/swagger"
echo ""
echo "Happy coding! 🚀"
