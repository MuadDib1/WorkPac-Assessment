using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using WorkPac.Recruitment.Contracts.ApiModels;
using WorkPac.Recruitment.Shared.Exceptions;

namespace WorkPac.Recruitment.Infrastructure.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
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
        catch (NotFoundException ex)
        {
            _logger.LogInformation("Resource not found: {ResourceType} {ResourceId}",
                ex.ResourceType, ex.ResourceId);
            await WriteProblemDetails(context, HttpStatusCode.NotFound,
                ex.Message, "https://httpstatuses.com/404");
        }
        catch (ConflictException ex)
        {
            _logger.LogWarning("Conflict: {Message}", ex.Message);
            await WriteProblemDetails(context, HttpStatusCode.Conflict,
                ex.Message, "https://httpstatuses.com/409");
        }
        catch (InvalidStatusTransitionException ex)
        {
            _logger.LogWarning("Invalid status transition: {From} -> {To}", ex.FromStatus, ex.ToStatus);
            await WriteProblemDetails(context, HttpStatusCode.UnprocessableEntity,
                ex.Message, "https://httpstatuses.com/422");
        }
        catch (ForbiddenException ex)
        {
            _logger.LogWarning("Forbidden: {Message}", ex.Message);
            await WriteProblemDetails(context, HttpStatusCode.Forbidden,
                ex.Message, "https://httpstatuses.com/403");
        }
        catch (ValidationException ex)
        {
            _logger.LogInformation("Validation failed: {Message}", ex.Message);
            await WriteProblemDetails(context, HttpStatusCode.BadRequest,
                ex.Message, "https://httpstatuses.com/400", ex.Errors);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning("Domain rule violated: {Message}", ex.Message);
            await WriteProblemDetails(context, HttpStatusCode.BadRequest,
                ex.Message, "https://httpstatuses.com/400");
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized access: {Message}", ex.Message);
            await WriteProblemDetails(context, HttpStatusCode.Unauthorized,
                "Authentication is required.", "https://httpstatuses.com/401");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception for {Path}", context.Request.Path);
            await WriteProblemDetails(context, HttpStatusCode.InternalServerError,
                "An unexpected error occurred.", "https://httpstatuses.com/500");
        }
    }

    private static async Task WriteProblemDetails(HttpContext context, HttpStatusCode status,
        string detail, string type, Dictionary<string, string[]>? errors = null)
    {
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)status;

        var problem = new ProblemDetail
        {
            Type = type,
            Title = Enum.GetName(status) ?? "Error",
            Status = (int)status,
            Detail = detail,
            Instance = context.Request.Path,
            CorrelationId = context.TraceIdentifier,
            Errors = errors
        };

        var json = JsonSerializer.Serialize(problem,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        await context.Response.WriteAsync(json);
    }
}
