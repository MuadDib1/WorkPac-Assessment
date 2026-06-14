using FluentValidation;
using WorkPac.Recruitment.Contracts.ApiModels;
using WorkPac.Recruitment.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "WorkPac Recruitment - Compliance API", Version = "v1" });
});

builder.Services.AddValidatorsFromAssemblyContaining<WorkPac.Recruitment.Compliance.Api.Validators.InitiateComplianceCheckValidator>();

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

v1.MapPost("/applications/{applicationId:guid}/compliance-checks", (
    Guid applicationId,
    InitiateComplianceCheckRequest request,
    IValidator<InitiateComplianceCheckRequest> validator) =>
{
    var validationResult = validator.Validate(request);
    if (!validationResult.IsValid)
        return Results.ValidationProblem(validationResult.ToDictionary());

    return Results.Ok(new ComplianceCheckResponse(
        Guid.NewGuid(),
        applicationId,
        "Pending",
        [
            new ComplianceCheckItem("Medical", "Pending", null),
            new ComplianceCheckItem("Induction", "Pending", null),
            new ComplianceCheckItem("License", "Pending", null)
        ]));
})
.WithName("InitiateComplianceCheck");

v1.MapGet("/applications/{applicationId:guid}/compliance-checks", (Guid applicationId) =>
{
    return Results.Ok(new ComplianceCheckResponse(
        Guid.Empty,
        applicationId,
        "Completed",
        [
            new ComplianceCheckItem("Medical", "Pass", DateTime.UtcNow),
            new ComplianceCheckItem("Induction", "Pending", null),
            new ComplianceCheckItem("License", "Pass", DateTime.UtcNow)
        ]));
})
.WithName("GetComplianceStatus");

app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Service = "Compliance", Timestamp = DateTime.UtcNow }));

app.Run();
