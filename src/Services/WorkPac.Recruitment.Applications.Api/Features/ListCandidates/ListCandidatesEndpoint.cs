namespace WorkPac.Recruitment.Applications.Api.Features.ListCandidates;

public static class ListCandidatesEndpoint
{
    public static void MapListCandidates(this WebApplication app)
    {
        app.MapGet("/v1/candidates", async (
            ListCandidatesHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(ct);
            return Results.Ok(result);
        })
        .WithName("ListCandidates")
        .WithOpenApi();
    }
}
