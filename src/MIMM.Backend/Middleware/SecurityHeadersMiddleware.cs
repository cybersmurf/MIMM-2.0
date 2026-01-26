using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace MIMM.Backend.Middleware;

/// <summary>
/// Middleware to add security headers to all HTTP responses.
/// Prevents common attacks: XSS, clickjacking, MIME sniffing, etc.
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Prevent MIME sniffing
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";

        // Prevent clickjacking
        context.Response.Headers["X-Frame-Options"] = "DENY";

        // XSS Protection (legacy, but still good for older browsers)
        context.Response.Headers["X-XSS-Protection"] = "1; mode=block";

        // Referrer Policy
        context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

        // Permissions Policy (formerly Feature-Policy)
        context.Response.Headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";

        // HSTS (only in production with HTTPS)
        if (context.Request.IsHttps)
        {
            context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
        }

        await _next(context);
    }
}
