using System.Net.Http;
using System.Security.Authentication;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TouristRoutes.Controllers;

[ApiController]
[ApiExplorerSettings(IgnoreApi = true)]
public class ErrorController : ControllerBase
{
    [Route("/error")]
    public IActionResult Error()
    {
        var exception = HttpContext.Features.Get<IExceptionHandlerFeature>()?.Error;
        if (exception == null)
        {
            return Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Server error",
                detail: "Unhandled error.",
                instance: HttpContext.Request.Path);
        }

        var (statusCode, title, detail) = exception switch
        {
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Not found", exception.Message),
            ValidationException => (StatusCodes.Status400BadRequest, "Validation failed", exception.Message),
            ArgumentException => (StatusCodes.Status400BadRequest, "Bad request", exception.Message),
            FormatException => (StatusCodes.Status400BadRequest, "Bad request", exception.Message),
            JsonException => (StatusCodes.Status400BadRequest, "Bad request", "Invalid JSON payload."),
            BadHttpRequestException => (StatusCodes.Status400BadRequest, "Bad request", exception.Message),
            AuthenticationException => (StatusCodes.Status401Unauthorized, "Unauthorized", "Authentication required."),
            UnauthorizedAccessException => (StatusCodes.Status403Forbidden, "Forbidden", "Access denied."),
            DbUpdateConcurrencyException => (StatusCodes.Status409Conflict, "Conflict", "Resource update conflict."),
            DbUpdateException => (StatusCodes.Status409Conflict, "Conflict", "Database update failed."),
            OperationCanceledException => (StatusCodes.Status408RequestTimeout, "Request timeout", "Request was canceled."),
            TimeoutException => (StatusCodes.Status503ServiceUnavailable, "Service unavailable", "Request timed out."),
            HttpRequestException => (StatusCodes.Status503ServiceUnavailable, "Service unavailable", "Upstream request failed."),
            _ => (StatusCodes.Status500InternalServerError, "Server error", "Unhandled error.")
        };

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            detail = "An unexpected error occurred.";
        }

        return Problem(
            statusCode: statusCode,
            title: title,
            detail: detail,
            instance: HttpContext.Request.Path);
    }
}