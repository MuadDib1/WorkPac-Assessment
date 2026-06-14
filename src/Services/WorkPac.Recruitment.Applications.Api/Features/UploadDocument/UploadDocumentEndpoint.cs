namespace WorkPac.Recruitment.Applications.Api.Features.UploadDocument;

public static class UploadDocumentEndpoint
{
    public static void MapUploadDocument(this WebApplication app)
    {
        app.MapPost("/v1/applications/{id:guid}/documents", async (
            Guid id,
            IFormFile file,
            UploadDocumentHandler handler,
            CancellationToken ct) =>
        {
            await using var stream = file.OpenReadStream();
            var result = await handler.HandleAsync(id, file.FileName, file.ContentType, stream, ct);
            return Results.Ok(result);
        })
        .WithName("UploadDocument")
        .DisableAntiforgery()
        .WithOpenApi();
    }
}
