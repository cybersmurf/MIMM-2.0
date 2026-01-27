# 🚀 VPS Deployment - Quick Reference Card

## ⚡ Problem & Solution

**Error on VPS:**
```
error : InvalidOperationException: Sequence contains more than one element
```

**Solution (2 commands):**
```bash
bash scripts/deep-clean-vps.sh
bash scripts/update-vps-frontend.sh
```

---

## 📋 Step-by-Step VPS Deployment

### 1. SSH to VPS
```bash
ssh user@musicinmymind.app
cd ~/mimm-app
```

### 2. Pull Latest Code
```bash
git pull origin main
```

### 3. Deep Clean (CRITICAL - First Time or After Errors)
```bash
bash scripts/deep-clean-vps.sh
```

### 4. Deploy Backend (Docker)
```bash
docker build -f Dockerfile -t mimm-backend:v2.0.1 .

docker run -d --name mimm-api --network=mimm-net \
  -p 8080:8080 -p 8081:8081 \
  -e "ConnectionStrings__DefaultConnection=Host=postgres;..." \
  -e "Jwt__Key=..." \
  mimm-backend:v2.0.1
```

### 5. Deploy Frontend
```bash
bash scripts/update-vps-frontend.sh
```

### 6. Verify
```bash
curl -k https://api.musicinmymind.app/health
curl -I https://musicinmymind.app/login
```

---

## 🔧 Troubleshooting Commands

### Check API Logs
```bash
docker logs mimm-api | tail -20
```

### Check Nginx Logs
```bash
sudo tail -20 /var/log/nginx/error.log
```

### Restart Services
```bash
docker restart mimm-api
sudo systemctl restart nginx
```

### Nuclear Clean (If All Else Fails)
```bash
pkill -f "dotnet"
find ~/mimm-app -type d -name "bin" -exec rm -rf {} + 2>/dev/null || true
find ~/mimm-app -type d -name "obj" -exec rm -rf {} + 2>/dev/null || true
dotnet nuget locals all -c
```

---

## 📚 Full Documentation

- **Quick Start:** `DEPLOYMENT_ACTION_PLAN.md`
- **StaticWebAssets Fix:** `docs/STATICWEBASSETS_FIX.md`
- **Comprehensive Guide:** `VPS_DEPLOYMENT_GUIDE.md`
- **Status Report:** `DEPLOYMENT_READY_STATUS.md`

---

## ✅ Success Checklist

- [ ] API health responds: `curl https://api.musicinmymind.app/health`
- [ ] Frontend loads: `curl -I https://musicinmymind.app/login`
- [ ] No errors in: `docker logs mimm-api | tail -20`
- [ ] No errors in: `sudo tail -20 /var/log/nginx/error.log`
- [ ] Browser: https://musicinmymind.app loads without blank screen
- [ ] Console (F12): No critical errors

---

## 🆘 Common Errors

| Error | Command to Fix |
|-------|----------------|
| Sequence contains more than one element | `bash scripts/deep-clean-vps.sh` |
| NETSDK1045 | Verify `dotnet --version` shows 9.0.x |
| Docker build fails | `docker system prune -a` |
| Nginx not serving | `sudo nginx -t && sudo systemctl restart nginx` |
| Database connection fails | `docker ps \| grep postgres` |

---

**Last Updated:** January 27, 2026  
**Version:** 2.0.1 (net9.0)
