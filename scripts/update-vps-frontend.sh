#!/bin/bash
set -e

cd ~/mimm-app

echo "=== MIMM 2.0 Frontend Update Script ==="
echo ""

echo "=== 1. Stashing local changes ==="
git stash push -m "Auto-stash before VPS frontend update" || echo "No local changes to stash"

echo ""
echo "=== 2. Pulling latest code from GitHub ==="
git pull origin main

echo ""
echo "=== 3. Deep clean build artifacts ==="
# Remove bin/obj directories
rm -rf src/MIMM.Frontend/bin src/MIMM.Frontend/obj
rm -rf src/MIMM.Shared/bin src/MIMM.Shared/obj

# Clean NuGet cache
dotnet nuget locals temp -c
echo "✅ Cleaned bin/obj/nuget cache"

echo ""
echo "=== 4. Restoring frontend dependencies ==="
dotnet restore "src/MIMM.Frontend/MIMM.Frontend.csproj"

echo ""
echo "=== 5. Publishing Blazor WASM frontend (Release) ==="
dotnet publish "src/MIMM.Frontend/MIMM.Frontend.csproj" \
    -c Release \
    -o ~/mimm-app/publish/frontend \
    --no-restore

echo ""
echo "=== 7. Restarting nginx ==="
sudo systemctl restart nginx || echo "Nginx restart may require elevated privileges"

echo ""
echo "=== 8. Checking nginx status ==="
sudo systemctl status nginx --no-pager | head -10 || echo "Could not check nginx status"

echo ""
echo "=== 9. Testing frontend endpoint ==="
curl -s -I https://musicinmymind.app/login 2>/dev/null | head -3 || echo "Frontend check - HTTPS may not be ready"

echo ""
echo "✅ Frontend update complete!"
echo ""
echo "Next steps:"
echo "  - Check: https://musicinmymind.app/login"
echo "  - Nginx logs: sudo tail -f /var/log/nginx/error.log"
echo "  - Clear browser cache if needed (Ctrl+Shift+Delete)"
