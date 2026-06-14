namespace WorkPac.Recruitment.Applications.Api.Features.ListJobs;

public static class ListJobsEndpoint
{
    public static void MapListJobs(this WebApplication app)
    {
        app.MapGet("/v1/jobs", async (
            ListJobsHandler handler,
            int? page,
            int? pageSize,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(page ?? 1, pageSize ?? 20, ct);
            return Results.Ok(result);
        })
        .WithName("ListJobs")
        .WithOpenApi();
    }
}
