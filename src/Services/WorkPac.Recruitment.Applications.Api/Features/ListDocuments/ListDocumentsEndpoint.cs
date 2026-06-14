namespace WorkPac.Recruitment.Applications.Api.Features.ListDocuments;

public static class ListDocumentsEndpoint
{
    public static void MapListDocuments(this WebApplication app)
    {
        app.MapGet("/v1/documents", async (
            ListDocumentsHandler handler,
            int? page,
            int? pageSize,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(page ?? 1, pageSize ?? 20, ct);
            return Results.Ok(result);
        })
        .WithName("ListDocuments")
        .WithOpenApi();

        app.MapGet("/v1/applications/{applicationId:guid}/documents", async (
            Guid applicationId,
            ListDocumentsHandler handler,
            int? page,
            int? pageSize,
            CancellationToken ct) =>
        {
            var result = await handler.HandleByApplicationAsync(applicationId, page ?? 1, pageSize ?? 20, ct);
            return Results.Ok(result);
        })
        .WithName("ListDocumentsByApplication")
        .WithOpenApi();
    }
}
