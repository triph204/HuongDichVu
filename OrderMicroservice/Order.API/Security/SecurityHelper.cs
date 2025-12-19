using Microsoft.AspNetCore.Http;

namespace Order.API.Security
{
    /// <summary>
    /// Security Helper - Các utility functions cho b?o m?t ? API Layer
    /// </summary>
    public static class SecurityHelper
    {
        /// <summary>
        /// L?y Client IP Address m?t cách an toàn
        /// X? lý c? tr??ng h?p có reverse proxy
        /// </summary>
        public static string GetClientIpAddress(HttpContext context)
        {
            if (context == null)
                return "unknown";

            // 1. Ki?m tra X-Forwarded-For header (khi có reverse proxy/load balancer)
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                var ip = forwardedFor.Split(',').FirstOrDefault()?.Trim();
                if (!string.IsNullOrEmpty(ip) && IsValidIpAddress(ip))
                    return ip;
            }

            // 2. Ki?m tra X-Real-IP header (nginx)
            var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp) && IsValidIpAddress(realIp))
                return realIp;

            // 3. Fallback to RemoteIpAddress
            var remoteIp = context.Connection.RemoteIpAddress?.ToString();
            if (!string.IsNullOrEmpty(remoteIp))
                return remoteIp;

            return "unknown";
        }

        /// <summary>
        /// L?y User ID t? claims (n?u ?ã authenticate)
        /// </summary>
        public static string? GetUserId(HttpContext context)
        {
            if (context?.User?.Identity?.IsAuthenticated != true)
                return null;

            return context.User.FindFirst("sub")?.Value ??
                   context.User.FindFirst("userId")?.Value ??
                   context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        }

        /// <summary>
        /// L?y User Role t? claims
        /// </summary>
        public static string GetUserRole(HttpContext context)
        {
            if (context?.User?.Identity?.IsAuthenticated != true)
                return "Anonymous";

            return context.User.FindFirst("role")?.Value ??
                   context.User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ??
                   "User";
        }

        /// <summary>
        /// Validate IP address format
        /// </summary>
        public static bool IsValidIpAddress(string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
                return false;

            return System.Net.IPAddress.TryParse(ipAddress, out _);
        }

        /// <summary>
        /// Ki?m tra request có ??n t? internal network không
        /// </summary>
        public static bool IsInternalRequest(HttpContext context)
        {
            var ip = GetClientIpAddress(context);
            
            if (ip == "127.0.0.1" || ip == "::1" || ip == "localhost")
                return true;

            // Ki?m tra private IP ranges
            if (System.Net.IPAddress.TryParse(ip, out var ipAddress))
            {
                var bytes = ipAddress.GetAddressBytes();
                
                // 10.x.x.x
                if (bytes[0] == 10)
                    return true;
                
                // 172.16.x.x - 172.31.x.x
                if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31)
                    return true;
                
                // 192.168.x.x
                if (bytes[0] == 192 && bytes[1] == 168)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Mask sensitive data cho logging (ví d?: s? ?i?n tho?i)
        /// </summary>
        public static string MaskSensitiveData(string data, int visibleChars = 4)
        {
            if (string.IsNullOrEmpty(data) || data.Length <= visibleChars)
                return "****";

            return new string('*', data.Length - visibleChars) + data.Substring(data.Length - visibleChars);
        }

        /// <summary>
        /// Generate secure random string (cho tokens, etc.)
        /// </summary>
        public static string GenerateSecureRandomString(int length = 32)
        {
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            var bytes = new byte[length];
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes).Substring(0, length);
        }
    }
}
