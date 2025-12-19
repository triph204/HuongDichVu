using Microsoft.AspNetCore.Http;

namespace Order.API.Middleware
{
    /// <summary>
    /// Security Headers Middleware - Thêm các HTTP security headers
    /// Phòng ch?ng XSS, Clickjacking, MIME sniffing attacks
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
            // ===== SECURITY HEADERS =====

            // 1. X-Content-Type-Options - Phòng ch?ng MIME sniffing
            context.Response.Headers["X-Content-Type-Options"] = "nosniff";

            // 2. X-Frame-Options - Phòng ch?ng Clickjacking
            context.Response.Headers["X-Frame-Options"] = "DENY";

            // 3. X-XSS-Protection - B?t XSS filter c?a browser
            context.Response.Headers["X-XSS-Protection"] = "1; mode=block";

            // 4. Referrer-Policy - Ki?m soát thông tin referrer
            context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

            // 5. Content-Security-Policy - Phòng ch?ng XSS và data injection
            context.Response.Headers["Content-Security-Policy"] = 
                "default-src 'self'; " +
                "script-src 'self'; " +
                "style-src 'self' 'unsafe-inline'; " +
                "img-src 'self' data: https:; " +
                "font-src 'self'; " +
                "frame-ancestors 'none'; " +
                "form-action 'self';";

            // 6. Permissions-Policy - Ki?m soát browser features
            context.Response.Headers["Permissions-Policy"] = 
                "accelerometer=(), camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), payment=(), usb=()";

            // 7. Strict-Transport-Security (HSTS) - B?t bu?c HTTPS
            // Ch? thêm khi s? d?ng HTTPS
            if (context.Request.IsHttps)
            {
                context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
            }

            // 8. Cache-Control cho sensitive endpoints
            if (IsSensitiveEndpoint(context.Request.Path))
            {
                context.Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate, proxy-revalidate";
                context.Response.Headers["Pragma"] = "no-cache";
                context.Response.Headers["Expires"] = "0";
            }

            // Remove server header ?? không expose server info
            context.Response.Headers.Remove("Server");
            context.Response.Headers.Remove("X-Powered-By");

            await _next(context);
        }

        private bool IsSensitiveEndpoint(PathString path)
        {
            var sensitivePaths = new[]
            {
                "/api/orders",
                "/api/auth",
                "/api/users"
            };

            return sensitivePaths.Any(p => path.StartsWithSegments(p, StringComparison.OrdinalIgnoreCase));
        }
    }

    /// <summary>
    /// Extension method ?? ??ng ký Security Headers Middleware
    /// </summary>
    public static class SecurityHeadersMiddlewareExtensions
    {
        public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SecurityHeadersMiddleware>();
        }
    }
}
