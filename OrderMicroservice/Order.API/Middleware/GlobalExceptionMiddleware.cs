using System.Net;
using Order.API.Models;

namespace Order.API.Middleware
{
    /// <summary>
    /// Global Exception Middleware - X? lý exceptions và b?o m?t thông tin
    /// Phòng ch?ng Information Disclosure vulnerability
    /// </summary>
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly IHostEnvironment _environment;

        public GlobalExceptionMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionMiddleware> logger,
            IHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                // Log ??y ?? thông tin exception (ch? trong server logs)
                _logger.LogError(ex,
                    "Unhandled exception occurred. TraceId: {TraceId}, Path: {Path}, Method: {Method}",
                    context.TraceIdentifier,
                    context.Request.Path,
                    context.Request.Method);

                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = new ApiResponse<object>
            {
                Success = false,
                TraceId = context.TraceIdentifier // Cho phép trace l?i mà không leak info
            };

            switch (exception)
            {
                // ===== VALIDATION ERRORS =====
                case ArgumentNullException argEx:
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    response.Message = "Thi?u thông tin b?t bu?c";
                    // KHÔNG expose tên parameter trong production
                    if (_environment.IsDevelopment())
                    {
                        response.Errors = new List<string> { $"Missing: {argEx.ParamName}" };
                    }
                    break;

                case ArgumentException:
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    response.Message = "D? li?u không h?p l?";
                    // KHÔNG expose chi ti?t argument error
                    break;

                // ===== NOT FOUND =====
                case KeyNotFoundException:
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    response.Message = "Không tìm th?y tài nguyên";
                    break;

                // ===== AUTHORIZATION ERRORS =====
                case UnauthorizedAccessException:
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    response.Message = "B?n không có quy?n th?c hi?n hành ??ng này";
                    break;

                // ===== BUSINESS LOGIC ERRORS =====
                case InvalidOperationException:
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    response.Message = "Không th? th?c hi?n hành ??ng này";
                    break;

                // ===== TIMEOUT =====
                case TimeoutException:
                    context.Response.StatusCode = StatusCodes.Status408RequestTimeout;
                    response.Message = "Yêu c?u quá th?i gian x? lý";
                    break;

                // ===== DEFAULT - INTERNAL SERVER ERROR =====
                default:
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    response.Message = "?ã x?y ra l?i. Vui lòng th? l?i sau.";

                    // CH? expose chi ti?t l?i trong Development
                    if (_environment.IsDevelopment())
                    {
                        response.Errors = new List<string>
                        {
                            exception.Message,
                            exception.StackTrace ?? string.Empty
                        };
                    }
                    // Production: KHÔNG expose exception message ho?c stack trace
                    break;
            }

            return context.Response.WriteAsJsonAsync(response);
        }
    }
}
