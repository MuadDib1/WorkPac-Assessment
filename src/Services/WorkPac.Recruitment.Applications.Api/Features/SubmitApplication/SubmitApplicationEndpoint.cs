using WorkPac.Recruitment.Applications.Api.Features.SubmitApplication;
using WorkPac.Recruitment.Contracts.ApiModels;

namespace WorkPac.Recruitment.Applications.Api.Features;

public static class SubmitApplicationEndpoint
{
    public static void MapSubmitApplication(this WebApplication app)
    {
        app.MapPost("/v1/jobs/{jobId:guid}/applications", async (
            Guid jobId,
            SubmitApplicationRequest request,
            SubmitApplicationHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(jobId, request, ct);
            return Results.Created($"/v1/applications/{result.Id}", result);
        })
        .WithName("SubmitApplication")
        .WithOpenApi();
    }
}
