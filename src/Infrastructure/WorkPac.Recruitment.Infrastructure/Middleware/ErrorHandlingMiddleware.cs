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
        catch (Exception ex) when (ex is not DomainException)
        {
            _logger.LogError(ex, "Unhandled exception for {Path}", context.Request.Path);
            await WriteProblemDetails(context, HttpStatusCode.InternalServerError,
                "An unexpected error occurred.", "https://httpstatuses.com/500");
        }
        catch (NotFoundException ex)
        {
            await WriteProblemDetails(context, HttpStatusCode.NotFound,
                ex.Message, "https://httpstatuses.com/404");
        }
        catch (ConflictException ex)
        {
            await WriteProblemDetails(context, HttpStatusCode.Conflict,
                ex.Message, "https://httpstatuses.com/409");
        }
        catch (InvalidStatusTransitionException ex)
        {
            await WriteProblemDetails(context, HttpStatusCode.UnprocessableEntity,
                ex.Message, "https://httpstatuses.com/422");
        }
        catch (ValidationException ex)
        {
            await WriteProblemDetails(context, HttpStatusCode.BadRequest,
                ex.Message, "https://httpstatuses.com/400");
        }
    }

    private static async Task WriteProblemDetails(HttpContext context, HttpStatusCode status,
        string detail, string type)
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
            CorrelationId = context.TraceIdentifier
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
    }
}
