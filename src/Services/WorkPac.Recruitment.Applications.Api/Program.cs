using FluentValidation;
using WorkPac.Recruitment.Applications.Api.Features;
using WorkPac.Recruitment.Applications.Api.Features.GetApplication;
using WorkPac.Recruitment.Applications.Api.Features.ListApplications;
using WorkPac.Recruitment.Applications.Api.Features.ListCandidates;
using WorkPac.Recruitment.Applications.Api.Features.ListJobs;
using WorkPac.Recruitment.Applications.Api.Features.SubmitApplication;
using WorkPac.Recruitment.Applications.Api.Features.UpdateApplicationStatus;
using WorkPac.Recruitment.Applications.Api.Features.UploadDocument;
using WorkPac.Recruitment.Applications.Api.Features.WithdrawApplication;
using WorkPac.Recruitment.Infrastructure;
using WorkPac.Recruitment.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "WorkPac Recruitment - Applications API", Version = "v1" });
});

builder.Services.AddValidatorsFromAssemblyContaining<SubmitApplicationValidator>();

builder.Services.AddScoped<SubmitApplicationHandler>();
builder.Services.AddScoped<GetApplicationHandler>();
builder.Services.AddScoped<UpdateApplicationStatusHandler>();
builder.Services.AddScoped<ListApplicationsHandler>();
builder.Services.AddScoped<ListJobsHandler>();
builder.Services.AddScoped<ListCandidatesHandler>();
builder.Services.AddScoped<UploadDocumentHandler>();
builder.Services.AddScoped<WithdrawApplicationHandler>();

var mode = builder.Configuration.GetValue<string>("InfrastructureMode") ?? "Local";
builder.Services.AddInfrastructure(builder.Configuration, mode);

var app = builder.Build();

app.UseInfrastructureMiddleware();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapSubmitApplication();
app.MapGetApplication();
app.MapUpdateApplicationStatus();
app.MapListApplications();
app.MapListJobs();
app.MapListCandidates();
app.MapUploadDocument();
app.MapWithdrawApplication();

app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }))
   .WithName("HealthCheck");

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
    await seeder.SeedAsync();
}

app.Run();

// Exposed for integration testing
public partial class Program { }
