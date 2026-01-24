# Prompt: Security Hardening Checklist

Goal: Identify and address security gaps before release.

Checklist:

- AuthN/AuthZ: JWT validity, issuer/audience, key length (>=256-bit), token expiry; ensure `[Authorize]` on protected endpoints.
- Secrets: No secrets in repo; use user-secrets/env vars; validate appsettings samples.
- CORS: Restrict allowed origins to frontend; allow credentials only if needed.
- HTTPS: Enforce in production; dev HTTPS redirection disabled only when necessary.
- Headers: Add HSTS, X-Content-Type-Options, X-Frame-Options, Referrer-Policy, Content-Security-Policy (if applicable).
- Input validation: DTO `required`, model validation; guard against over-posting and large payloads.
- Logging: Avoid PII in logs; sanitize exceptions; structured logging (Serilog).
- Rate limiting/throttling: consider for auth endpoints.
- Dependency scan: check `dotnet list package --vulnerable`.

Actions:

1. Scan code for missing `[Authorize]` and weak CORS.
2. Verify JWT configuration and key length.
3. Propose CSP/headers middleware and minimal configuration.
4. Report findings as bullets (severity + file/link).
