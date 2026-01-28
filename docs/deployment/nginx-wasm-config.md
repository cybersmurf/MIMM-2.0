# Nginx WASM Configuration

**Date:** 2026-01-28  
**Issue:** WASM Content-Type warning in browser console

## Problem

```
MONO_WASM: WebAssembly resource does not have the expected content type "application/wasm", 
so falling back to slower ArrayBuffer instantiation.
```

## Root Cause

Nginx was serving `.wasm` files with `Content-Type: text/html` instead of `application/wasm`, preventing WebAssembly streaming instantiation.

## Solution

Updated `/etc/nginx/sites-available/mimm-frontend` with explicit WASM handling:

```nginx
# WASM files (MUST be before _framework/ to override)
location ~* \.wasm$ {
    types { }
    default_type application/wasm;
    add_header Cache-Control "public, max-age=31536000, immutable";
}

# Brotli compressed WASM (.wasm.br)
location ~* \.wasm\.br$ {
    types { }
    default_type application/wasm;
    add_header Content-Encoding "br";
    add_header Cache-Control "public, max-age=31536000, immutable";
}

# Gzip compressed WASM (.wasm.gz)
location ~* \.wasm\.gz$ {
    types { }
    default_type application/wasm;
    add_header Content-Encoding "gzip";
    add_header Cache-Control "public, max-age=31536000, immutable";
}
```

### Key Changes

1. **`types { }` + `default_type`**: Explicitly override MIME type detection
2. **Position matters**: WASM rules placed **before** `_framework/` location block
3. **Compression support**: Added `.wasm.br` and `.wasm.gz` variants for pre-compressed files
4. **Aggressive caching**: `max-age=31536000, immutable` (1 year) for versioned WASM files

## Verification

```bash
# Check Content-Type
curl -I https://musicinmymind.app/_framework/dotnet.native.*.wasm | grep Content-Type

# Expected output:
Content-Type: application/wasm
```

## Deployment Steps

```bash
# 1. SSH to VPS
ssh -p 2222 mimm@188.245.68.164

# 2. Edit nginx config (with sudo)
sudo nano /etc/nginx/sites-available/mimm-frontend

# 3. Test config
sudo nginx -t

# 4. Reload nginx
sudo systemctl reload nginx

# 5. Verify
curl -I https://musicinmymind.app/_framework/dotnet.native.*.wasm | grep Content-Type
```

## Result

- ✅ WebAssembly streaming instantiation enabled
- ✅ Faster WASM module loading
- ✅ Console warning eliminated
- ✅ Browser DevTools no longer show yellow warnings

## References

- [MDN: WebAssembly.instantiateStreaming()](https://developer.mozilla.org/en-US/docs/WebAssembly/JavaScript_interface/instantiateStreaming_static)
- [Blazor WASM Performance Best Practices](https://learn.microsoft.com/en-us/aspnet/core/blazor/performance)
- Nginx MIME types: `/etc/nginx/mime.types`
