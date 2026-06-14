namespace WorkPac.Recruitment.Applications.Api.Features.GetApplication;

public static class GetApplicationEndpoint
{
    public static void MapGetApplication(this WebApplication app)
    {
        app.MapGet("/v1/applications/{id:guid}", async (
            Guid id,
            GetApplicationHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(id, ct);
            return Results.Ok(result);
        })
        .WithName("GetApplication")
        .WithOpenApi();
    }
}
