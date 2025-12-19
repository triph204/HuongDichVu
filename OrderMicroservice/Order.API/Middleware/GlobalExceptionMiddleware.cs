using System.Net;
using Order.API.Models;

namespace Order.API.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            var response = new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred processing your request"
            };

            switch (exception)
            {
                case ArgumentNullException argEx:
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    response.Message = $"Missing required parameter: {argEx.ParamName}";
                    break;

                case ArgumentException argEx:
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    response.Message = argEx.Message;
                    break;

                case KeyNotFoundException:
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    response.Message = "Resource not found";
                    break;

                default:
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    response.Message = "Internal server error";
                    response.Errors = new List<string> { exception.Message };
                    break;
            }

            return context.Response.WriteAsJsonAsync(response);
        }
    }
}
