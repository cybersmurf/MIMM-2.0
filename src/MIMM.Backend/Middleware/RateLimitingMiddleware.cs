using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Serilog;

namespace MIMM.Backend.Middleware;

/// <summary>
/// Simple in-memory rate limiting middleware.
/// Tracks requests per IP address and enforces limits on auth endpoints.
/// For production, use Azure API Management or dedicated rate limiting service.
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;

    // Rate limiting rules: endpoint pattern -> (requests per window, window in seconds)
    private static readonly Dictionary<string, (int limit, int windowSeconds)> RateLimitRules = new()
    {
        { "/api/auth/register", (5, 3600) },      // 5 registrations per hour per IP
        { "/api/auth/login", (10, 300) },         // 10 login attempts per 5 minutes per IP
        { "/api/auth/refresh", (30, 3600) },      // 30 token refreshes per hour per IP
    };

    public RateLimitingMiddleware(RequestDelegate next, IMemoryCache cache)
    {
        _next = next;
        _cache = cache;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "";
        var isRateLimited = RateLimitRules.Keys.Any(rule => path.StartsWith(rule));

        if (isRateLimited)
        {
            var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var rateLimitKey = $"ratelimit:{clientIp}:{path}";

            if (_cache.TryGetValue(rateLimitKey, out int requestCount))
            {
                var rule = RateLimitRules.First(r => path.StartsWith(r.Key));
                if (requestCount >= rule.Value.limit)
                {
                    Log.Warning("Rate limit exceeded for IP {Ip} on {Path}", clientIp, path);
                    context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        error = "Too many requests",
                        message = $"Rate limit exceeded. Please try again in {rule.Value.windowSeconds} seconds.",
                        retryAfter = rule.Value.windowSeconds
                    });
                    return;
                }

                _cache.Set(rateLimitKey, requestCount + 1, TimeSpan.FromSeconds(rule.Value.windowSeconds));
            }
            else
            {
                var rule = RateLimitRules.First(r => path.StartsWith(r.Key));
                _cache.Set(rateLimitKey, 1, TimeSpan.FromSeconds(rule.Value.windowSeconds));
            }
        }

        await _next(context);
    }
}
