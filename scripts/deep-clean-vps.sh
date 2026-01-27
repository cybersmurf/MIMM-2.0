#!/bin/bash
# MIMM 2.0 - VPS StaticWebAssets Cleanup Script
# Fixes "Sequence contains more than one element" error

set -e

cd ~/mimm-app

echo "=== MIMM 2.0 Deep Clean (StaticWebAssets Fix) ==="
echo ""

echo "=== 1. Stopping any running dotnet processes ==="
pkill -f "dotnet" || echo "No dotnet processes running"
sleep 1

echo ""
echo "=== 2. Removing ALL bin/obj directories ==="
find src tests -type d -name "bin" -exec rm -rf {} + 2>/dev/null || true
find src tests -type d -name "obj" -exec rm -rf {} + 2>/dev/null || true
echo "✅ Removed all bin/obj directories"

echo ""
echo "=== 3. Cleaning NuGet caches ==="
dotnet nuget locals all -c
echo "✅ NuGet caches cleared"

echo ""
echo "=== 4. Verifying clean state ==="
echo "Remaining bin/obj directories:"
find src tests -type d \( -name "bin" -o -name "obj" \) 2>/dev/null || echo "  (none - clean!)"

echo ""
echo "✅ Deep clean complete! Now run:"
echo "   bash scripts/update-vps-frontend.sh"
