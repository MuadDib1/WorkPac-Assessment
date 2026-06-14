using WorkPac.Recruitment.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "WorkPac Recruitment - Compliance API", Version = "v1" });
});

var mode = builder.Configuration.GetValue<string>("InfrastructureMode") ?? "Local";
builder.Services.AddInfrastructure(builder.Configuration, mode);

var app = builder.Build();

app.UseInfrastructureMiddleware();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var v1 = app.MapGroup("/v1");

v1.MapPost("/applications/{applicationId:guid}/compliance-checks", (Guid applicationId) =>
{
    return Results.Ok(new
    {
        Id = Guid.NewGuid(),
        ApplicationId = applicationId,
        Status = "Pending",
        Checks = new object[]
        {
            new { Type = "Medical", Status = "Pending" },
            new { Type = "Induction", Status = "Pending" },
            new { Type = "License", Status = "Pending" }
        }
    });
})
.WithName("InitiateComplianceCheck");

v1.MapGet("/applications/{applicationId:guid}/compliance-checks", (Guid applicationId) =>
{
    return Results.Ok(new
    {
        ApplicationId = applicationId,
        Checks = new object[]
        {
            new { Type = "Medical", Status = "Pass", VerifiedAt = (DateTime?)DateTime.UtcNow },
            new { Type = "Induction", Status = "Pending", VerifiedAt = (DateTime?)null },
            new { Type = "License", Status = "Pass", VerifiedAt = (DateTime?)DateTime.UtcNow }
        }
    });
})
.WithName("GetComplianceStatus");

app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Service = "Compliance", Timestamp = DateTime.UtcNow }));

app.Run();
