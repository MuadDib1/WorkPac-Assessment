namespace WorkPac.Recruitment.Applications.Api.Features.ListJobs;

public static class ListJobsEndpoint
{
    public static void MapListJobs(this WebApplication app)
    {
        app.MapGet("/v1/jobs", async (
            ListJobsHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(ct);
            return Results.Ok(result);
        })
        .WithName("ListJobs")
        .WithOpenApi();
    }
}
