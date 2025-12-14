using System.Net;
using System.Text.Json;

namespace CommunityFinanceAPI.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger,
            IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            var response = context.Response;

            var errorResponse = new ErrorResponse
            {
                Success = false,
                Message = exception.Message
            };

            switch (exception)
            {
                case UnauthorizedAccessException:
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    errorResponse.Message = "Unauthorized access";
                    break;

                case KeyNotFoundException:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    errorResponse.Message = exception.Message;
                    break;

                case InvalidOperationException:
                case ArgumentException:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse.Message = exception.Message;
                    break;

                default:
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    errorResponse.Message = _env.IsDevelopment() ? exception.Message : "An internal server error occurred";
                    if (_env.IsDevelopment())
                    {
                        errorResponse.StackTrace = exception.StackTrace;
                        errorResponse.InnerException = exception.InnerException?.Message;
                    }
                    break;
            }

            var result = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(result);
        }
    }

    public class ErrorResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? StackTrace { get; set; }
        public string? InnerException { get; set; }
    }

    public static class ExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionHandlingMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}