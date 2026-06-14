namespace WorkPac.Recruitment.Applications.Api.Features.ListApplications;

public static class ListApplicationsEndpoint
{
    public static void MapListApplications(this WebApplication app)
    {
        app.MapGet("/v1/jobs/{jobId:guid}/applications", async (
            Guid jobId,
            ListApplicationsHandler handler,
            int? page,
            int? pageSize,
            CancellationToken ct) =>
        {
            var result = await handler.HandleByJobAsync(jobId, page ?? 1, pageSize ?? 20, ct);
            return Results.Ok(result);
        })
        .WithName("ListApplicationsByJob")
        .WithOpenApi();

        app.MapGet("/v1/candidates/{candidateId:guid}/applications", async (
            Guid candidateId,
            ListApplicationsHandler handler,
            int? page,
            int? pageSize,
            CancellationToken ct) =>
        {
            var result = await handler.HandleByCandidateAsync(candidateId, page ?? 1, pageSize ?? 20, ct);
            return Results.Ok(result);
        })
        .WithName("ListApplicationsByCandidate")
        .WithOpenApi();
    }
}
