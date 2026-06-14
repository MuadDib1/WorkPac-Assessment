namespace WorkPac.Recruitment.Applications.Api.Features.WithdrawApplication;

public static class WithdrawApplicationEndpoint
{
    public static void MapWithdrawApplication(this WebApplication app)
    {
        app.MapDelete("/v1/applications/{id:guid}", async (
            Guid id,
            WithdrawApplicationHandler handler,
            CancellationToken ct) =>
        {
            await handler.HandleAsync(id, ct);
            return Results.NoContent();
        })
        .WithName("WithdrawApplication")
        .WithOpenApi();
    }
}
