namespace WorkPac.Recruitment.Applications.Api.Features.ListApplications;

public static class ListApplicationsEndpoint
{
    public static void MapListApplications(this WebApplication app)
    {
        app.MapGet("/v1/jobs/{jobId:guid}/applications", async (
            Guid jobId,
            ListApplicationsHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleByJobAsync(jobId, ct);
            return Results.Ok(result);
        })
        .WithName("ListApplicationsByJob")
        .WithOpenApi();

        app.MapGet("/v1/candidates/{candidateId:guid}/applications", async (
            Guid candidateId,
            ListApplicationsHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleByCandidateAsync(candidateId, ct);
            return Results.Ok(result);
        })
        .WithName("ListApplicationsByCandidate")
        .WithOpenApi();
    }
}
