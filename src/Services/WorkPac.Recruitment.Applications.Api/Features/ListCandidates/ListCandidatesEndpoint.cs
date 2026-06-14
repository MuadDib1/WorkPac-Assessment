namespace WorkPac.Recruitment.Applications.Api.Features.ListCandidates;

public static class ListCandidatesEndpoint
{
    public static void MapListCandidates(this WebApplication app)
    {
        app.MapGet("/v1/candidates", async (
            ListCandidatesHandler handler,
            int? page,
            int? pageSize,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(page ?? 1, pageSize ?? 20, ct);
            return Results.Ok(result);
        })
        .WithName("ListCandidates")
        .WithOpenApi();
    }
}
