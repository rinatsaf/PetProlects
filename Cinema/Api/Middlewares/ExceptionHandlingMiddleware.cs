using System.Net;
using System.Text.Json;
using Application.Exceptions;

namespace Api.Middlewares;

public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled exception occurred.");

            await HandleExceptionAsync(context, exception);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message) = exception switch
        {
            ValidationException ex => ((int)HttpStatusCode.BadRequest, ex.Message),
            NotFoundException ex => ((int)HttpStatusCode.NotFound, ex.Message),
            ConflictException ex => ((int)HttpStatusCode.Conflict, ex.Message),
            ForbiddenException ex => ((int)HttpStatusCode.Forbidden, ex.Message),
            UnauthorizedAccessException ex => ((int)HttpStatusCode.Unauthorized, ex.Message),
            RateLimitExceededException ex => (StatusCodes.Status429TooManyRequests, ex.Message),
            ExternalServiceException ex => (StatusCodes.Status502BadGateway, ex.Message),
            _ => ((int)HttpStatusCode.InternalServerError, "An unexpected error occurred.")
        };

        context.Response.StatusCode = statusCode;

        var response = new
        {
            statusCode,
            message
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
