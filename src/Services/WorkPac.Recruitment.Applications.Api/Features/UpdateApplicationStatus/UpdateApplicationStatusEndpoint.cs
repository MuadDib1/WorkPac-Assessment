using WorkPac.Recruitment.Contracts.ApiModels;

namespace WorkPac.Recruitment.Applications.Api.Features.UpdateApplicationStatus;

public static class UpdateApplicationStatusEndpoint
{
    public static void MapUpdateApplicationStatus(this WebApplication app)
    {
        app.MapPatch("/v1/applications/{id:guid}/status", async (
            Guid id,
            UpdateApplicationStatusRequest request,
            UpdateApplicationStatusHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(id, request, ct);
            return Results.Ok(result);
        })
        .WithName("UpdateApplicationStatus")
        .WithOpenApi();
    }
}
