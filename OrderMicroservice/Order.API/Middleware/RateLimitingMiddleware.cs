using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System.Net;

namespace Order.API.Middleware
{
    /// <summary>
    /// Rate Limiting Middleware - Phòng ch?ng DDoS và Brute Force attacks
    /// Gi?i h?n s? request t? m?i IP trong m?t kho?ng th?i gian
    /// </summary>
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;
        private readonly ILogger<RateLimitingMiddleware> _logger;

        // C?u hình rate limiting
        private const int MaxRequestsPerMinute = 100;  // S? request t?i ?a m?i phút
        private const int MaxRequestsPerSecond = 10;   // S? request t?i ?a m?i giây
        private const int BlockDurationMinutes = 5;     // Th?i gian block n?u v??t limit

        public RateLimitingMiddleware(
            RequestDelegate next,
            IMemoryCache cache,
            ILogger<RateLimitingMiddleware> logger)
        {
            _next = next;
            _cache = cache;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var clientIp = GetClientIpAddress(context);

            // Ki?m tra IP có b? block không
            var blockKey = $"blocked_{clientIp}";
            if (_cache.TryGetValue(blockKey, out _))
            {
                _logger.LogWarning("Blocked IP attempted access: {ClientIp}", clientIp);
                context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                context.Response.Headers["Retry-After"] = (BlockDurationMinutes * 60).ToString();
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Too Many Requests",
                    message = "B?n ?ã g?i quá nhi?u request. Vui lòng th? l?i sau.",
                    retryAfterMinutes = BlockDurationMinutes
                });
                return;
            }

            // Ki?m tra rate limit per second
            var secondKey = $"rate_second_{clientIp}_{DateTime.UtcNow:yyyyMMddHHmmss}";
            var secondCount = _cache.GetOrCreate(secondKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(1);
                return 0;
            });

            if (secondCount >= MaxRequestsPerSecond)
            {
                _logger.LogWarning("Rate limit exceeded (per second) for IP: {ClientIp}", clientIp);
                await HandleRateLimitExceeded(context, clientIp, blockKey);
                return;
            }

            _cache.Set(secondKey, secondCount + 1, TimeSpan.FromSeconds(1));

            // Ki?m tra rate limit per minute
            var minuteKey = $"rate_minute_{clientIp}_{DateTime.UtcNow:yyyyMMddHHmm}";
            var minuteCount = _cache.GetOrCreate(minuteKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
                return 0;
            });

            if (minuteCount >= MaxRequestsPerMinute)
            {
                _logger.LogWarning("Rate limit exceeded (per minute) for IP: {ClientIp}", clientIp);
                await HandleRateLimitExceeded(context, clientIp, blockKey);
                return;
            }

            _cache.Set(minuteKey, minuteCount + 1, TimeSpan.FromMinutes(1));

            // Thêm rate limit headers vào response
            context.Response.OnStarting(() =>
            {
                context.Response.Headers["X-RateLimit-Limit"] = MaxRequestsPerMinute.ToString();
                context.Response.Headers["X-RateLimit-Remaining"] = Math.Max(0, MaxRequestsPerMinute - minuteCount - 1).ToString();
                return Task.CompletedTask;
            });

            await _next(context);
        }

        private async Task HandleRateLimitExceeded(HttpContext context, string clientIp, string blockKey)
        {
            // Block IP trong m?t kho?ng th?i gian
            _cache.Set(blockKey, true, TimeSpan.FromMinutes(BlockDurationMinutes));

            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            context.Response.Headers["Retry-After"] = (BlockDurationMinutes * 60).ToString();
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Too Many Requests",
                message = "Quá nhi?u request. IP c?a b?n ?ã b? t?m block.",
                retryAfterMinutes = BlockDurationMinutes
            });
        }

        private string GetClientIpAddress(HttpContext context)
        {
            // ?u tiên X-Forwarded-For header (khi có reverse proxy)
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                // L?y IP ??u tiên trong danh sách
                var ip = forwardedFor.Split(',').FirstOrDefault()?.Trim();
                if (!string.IsNullOrEmpty(ip))
                    return ip;
            }

            // Fallback to RemoteIpAddress
            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }

    /// <summary>
    /// Extension method ?? ??ng ký Rate Limiting Middleware
    /// </summary>
    public static class RateLimitingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RateLimitingMiddleware>();
        }
    }
}
