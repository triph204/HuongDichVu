using System.Text.RegularExpressions;
using System.Web;

namespace Order.Application.Security
{
    /// <summary>
    /// Input Sanitizer - Phòng ch?ng XSS, SQL Injection, Script Injection
    /// </summary>
    public static class InputSanitizer
    {
        /// <summary>
        /// Sanitize string input ?? phòng ch?ng XSS
        /// Encode HTML entities và remove potentially dangerous characters
        /// </summary>
        public static string SanitizeString(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // 1. Trim whitespace
            var sanitized = input.Trim();

            // 2. HTML Encode ?? phòng ch?ng XSS
            sanitized = HttpUtility.HtmlEncode(sanitized);

            // 3. Remove script tags
            sanitized = Regex.Replace(sanitized, @"<script[^>]*>.*?</script>", "", 
                RegexOptions.IgnoreCase | RegexOptions.Singleline);

            // 4. Remove potentially dangerous HTML tags
            sanitized = Regex.Replace(sanitized, @"<[^>]*(javascript|onclick|onerror|onload)[^>]*>", "", 
                RegexOptions.IgnoreCase);

            // 5. Limit length to prevent DoS
            if (sanitized.Length > 5000)
            {
                sanitized = sanitized.Substring(0, 5000);
            }

            return sanitized;
        }

        /// <summary>
        /// Sanitize order number - ch? cho phép alphanumeric và dash
        /// </summary>
        public static string SanitizeOrderNumber(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Ch? gi? l?i ch?, s? và d?u g?ch ngang
            return Regex.Replace(input, @"[^a-zA-Z0-9\-]", "");
        }

        /// <summary>
        /// Validate và sanitize table name
        /// </summary>
        public static string SanitizeTableName(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            var sanitized = input.Trim();

            // Remove special characters except space và s?
            sanitized = Regex.Replace(sanitized, @"[^a-zA-Z0-9\s]", "");

            // Limit length
            if (sanitized.Length > 50)
            {
                sanitized = sanitized.Substring(0, 50);
            }

            return sanitized;
        }

        /// <summary>
        /// Validate và sanitize dish name
        /// </summary>
        public static string SanitizeDishName(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            var sanitized = input.Trim();

            // HTML encode
            sanitized = HttpUtility.HtmlEncode(sanitized);

            // Limit length
            if (sanitized.Length > 200)
            {
                sanitized = sanitized.Substring(0, 200);
            }

            return sanitized;
        }

        /// <summary>
        /// Validate number input ?? phòng ch?ng overflow
        /// </summary>
        public static bool IsValidPositiveInteger(int value, int maxValue = int.MaxValue)
        {
            return value > 0 && value <= maxValue;
        }

        /// <summary>
        /// Validate decimal input ?? phòng ch?ng overflow
        /// </summary>
        public static bool IsValidPositiveDecimal(decimal value, decimal maxValue = 999999999.99m)
        {
            return value > 0 && value <= maxValue;
        }

        /// <summary>
        /// Remove null bytes ?? phòng ch?ng null byte injection
        /// </summary>
        public static string RemoveNullBytes(string? input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            return input.Replace("\0", "");
        }

        /// <summary>
        /// Validate status string - ch? cho phép các status h?p l?
        /// </summary>
        public static bool IsValidStatus(string? status, HashSet<string> validStatuses)
        {
            if (string.IsNullOrWhiteSpace(status))
                return false;

            return validStatuses.Contains(status);
        }
    }
}
