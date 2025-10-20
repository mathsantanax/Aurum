using System.Net;
using System.Text.Json;

namespace AurumApi.Exceptions
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

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogWarning(ex, "Parâmetro nulo detectado");
                await HandleExceptionAsync(context, HttpStatusCode.BadRequest, ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Parâmetro inválido detectado");
                await HandleExceptionAsync(context, HttpStatusCode.BadRequest, ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Recurso não encontrado");
                await HandleExceptionAsync(context, HttpStatusCode.NotFound, ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Operação inválida");
                await HandleExceptionAsync(context, HttpStatusCode.Conflict, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado");
                await HandleExceptionAsync(context, HttpStatusCode.InternalServerError, "Ocorreu um erro inesperado. Tente novamente mais tarde.");
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, HttpStatusCode statusCode, string message)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var result = JsonSerializer.Serialize(new
            {
                status = statusCode,
                error = message
            });

            return context.Response.WriteAsync(result);
        }
    }
}
