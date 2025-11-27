    using Aurum.Applications.Exceptions;
using System.Text.Json;

namespace Aurum.API.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (AppException ex)
            {
                _logger.LogWarning(ex, $"[AppException] Status {ex.StatusCode}: {ex.Message}");
                await HandleExceptionAsync(httpContext, ex.StatusCode, ex.Message);
            }
            catch (Exception ex)
            {
                int statusCode = ex switch
                {
                    KeyNotFoundException => 404,
                    UnauthorizedAccessException => 401,
                    _ => 500
                };

                _logger.LogError(ex, $"[Critical Error] Status {statusCode}: {ex.Message}");

                await HandleExceptionAsync(httpContext, statusCode, "Ocorreu um erro interno no servidor.");
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, int statusCode, string message)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var result = JsonSerializer.Serialize(new
            {
                status = statusCode,
                error = message
            });

            return context.Response.WriteAsync(result);
        }
    }
}
