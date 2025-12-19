using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace Order.API.Middleware
{
    /// <summary>
    /// Request Validation Middleware - Ki?m tra và l?c các request ??c h?i
    /// Phòng ch?ng SQL Injection, XSS, Path Traversal, etc.
    /// </summary>
    public class RequestValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestValidationMiddleware> _logger;

        // Các pattern nguy hi?m c?n block
        private static readonly string[] DangerousPatterns = new[]
        {
            "<script",
            "javascript:",
            "onerror=",
            "onclick=",
            "onload=",
            "onmouseover=",
            "onfocus=",
            "eval(",
            "expression(",
            "../",           // Path traversal
            "..\\",          // Path traversal Windows
            "union select",  // SQL Injection
            "drop table",    // SQL Injection
            "delete from",   // SQL Injection
            "insert into",   // SQL Injection
            "exec(",         // SQL Injection
            "xp_",           // SQL Server injection
            "sp_",           // SQL Server injection
            "cmd.exe",       // Command injection
            "/bin/",         // Command injection
            "powershell",    // Command injection
            "%00",           // Null byte injection
            "&#",            // HTML entity encoding attack
            "\\x",           // Hex encoding attack
        };

        // Max request body size (5MB)
        private const long MaxRequestBodySize = 5 * 1024 * 1024;

        public RequestValidationMiddleware(
            RequestDelegate next, 
            ILogger<RequestValidationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // 1. Validate Request Method
            if (!IsValidHttpMethod(context.Request.Method))
            {
                _logger.LogWarning("Invalid HTTP method: {Method} from {IP}", 
                    context.Request.Method, 
                    context.Connection.RemoteIpAddress);
                context.Response.StatusCode = StatusCodes.Status405MethodNotAllowed;
                await context.Response.WriteAsJsonAsync(new { error = "Method not allowed" });
                return;
            }

            // 2. Validate Content-Length
            if (context.Request.ContentLength > MaxRequestBodySize)
            {
                _logger.LogWarning("Request too large: {Size} bytes from {IP}", 
                    context.Request.ContentLength, 
                    context.Connection.RemoteIpAddress);
                context.Response.StatusCode = StatusCodes.Status413RequestEntityTooLarge;
                await context.Response.WriteAsJsonAsync(new { error = "Request body too large" });
                return;
            }

            // 3. Validate Query String
            var queryString = context.Request.QueryString.Value ?? string.Empty;
            if (ContainsDangerousPatterns(queryString))
            {
                _logger.LogWarning("Dangerous pattern in query string from {IP}: {Query}", 
                    context.Connection.RemoteIpAddress, 
                    queryString);
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new { error = "Invalid request" });
                return;
            }

            // 4. Validate Path
            var path = context.Request.Path.Value ?? string.Empty;
            if (ContainsDangerousPatterns(path))
            {
                _logger.LogWarning("Dangerous pattern in path from {IP}: {Path}", 
                    context.Connection.RemoteIpAddress, 
                    path);
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new { error = "Invalid request path" });
                return;
            }

            // 5. Validate Headers
            foreach (var header in context.Request.Headers)
            {
                if (ContainsDangerousPatterns(header.Value.ToString()))
                {
                    _logger.LogWarning("Dangerous pattern in header {Header} from {IP}", 
                        header.Key, 
                        context.Connection.RemoteIpAddress);
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsJsonAsync(new { error = "Invalid request header" });
                    return;
                }
            }

            // 6. Validate Request Body (for POST, PUT, PATCH)
            if (HasRequestBody(context.Request.Method))
            {
                context.Request.EnableBuffering();

                using var reader = new StreamReader(
                    context.Request.Body, 
                    Encoding.UTF8, 
                    detectEncodingFromByteOrderMarks: false, 
                    leaveOpen: true);

                var body = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;

                if (ContainsDangerousPatterns(body))
                {
                    _logger.LogWarning("Dangerous pattern in request body from {IP}", 
                        context.Connection.RemoteIpAddress);
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsJsonAsync(new { error = "Invalid request body" });
                    return;
                }

                // Validate JSON format
                if (context.Request.ContentType?.Contains("application/json") == true)
                {
                    if (!IsValidJson(body))
                    {
                        _logger.LogWarning("Invalid JSON in request body from {IP}", 
                            context.Connection.RemoteIpAddress);
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        await context.Response.WriteAsJsonAsync(new { error = "Invalid JSON format" });
                        return;
                    }
                }
            }

            await _next(context);
        }

        private static bool IsValidHttpMethod(string method)
        {
            return method switch
            {
                "GET" or "POST" or "PUT" or "DELETE" or "PATCH" or "OPTIONS" or "HEAD" => true,
                _ => false
            };
        }

        private static bool HasRequestBody(string method)
        {
            return method is "POST" or "PUT" or "PATCH";
        }

        private static bool ContainsDangerousPatterns(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;

            var lowerInput = input.ToLowerInvariant();
            
            foreach (var pattern in DangerousPatterns)
            {
                if (lowerInput.Contains(pattern.ToLowerInvariant()))
                    return true;
            }

            return false;
        }

        private static bool IsValidJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return true; // Empty body is valid

            try
            {
                JsonDocument.Parse(json);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Extension method ?? ??ng ký Request Validation Middleware
    /// </summary>
    public static class RequestValidationMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestValidation(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestValidationMiddleware>();
        }
    }
}
