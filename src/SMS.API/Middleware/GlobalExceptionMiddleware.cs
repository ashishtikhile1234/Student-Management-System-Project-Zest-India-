using System.Net;
using System.Text.Json;
using SMS.Application.DTOs;
using SMS.Domain.Exceptions;

namespace SMS.API.Middleware;

/// <summary>
/// Global exception handling middleware.
/// Catches all unhandled exceptions, logs them via Serilog,
/// and returns a consistent JSON error response.
/// Registered once in Program.cs — no try/catch needed in controllers.
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next   = next;
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
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Determine HTTP status code based on exception type
        var statusCode = exception switch
        {
            NotFoundException           => HttpStatusCode.NotFound,
            ConflictException           => HttpStatusCode.Conflict,
            SMS.Domain.Exceptions.ValidationException => HttpStatusCode.BadRequest,
            UnauthorizedAccessException => HttpStatusCode.Unauthorized,
            _                           => HttpStatusCode.InternalServerError
        };

        // Log at appropriate level
        if (statusCode == HttpStatusCode.InternalServerError)
            _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
        else
            _logger.LogWarning("Handled exception [{StatusCode}]: {Message}", (int)statusCode, exception.Message);

        // Build standard response
        var response = new
        {
            success    = false,
            statusCode = (int)statusCode,
            message    = statusCode == HttpStatusCode.InternalServerError
                         ? "An unexpected error occurred. Please try again later."
                         : exception.Message,
            traceId    = context.TraceIdentifier
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode  = (int)statusCode;

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
