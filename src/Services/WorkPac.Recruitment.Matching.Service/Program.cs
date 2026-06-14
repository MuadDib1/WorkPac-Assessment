using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using WorkPac.Recruitment.Infrastructure;
using WorkPac.Recruitment.Matching.Service;
using WorkPac.Recruitment.Matching.Service.Consumers;
using WorkPac.Recruitment.Matching.Service.Scoring;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<SkillsScorer>();
builder.Services.AddSingleton<ExperienceScorer>();
builder.Services.AddSingleton<LocationScorer>();
builder.Services.AddSingleton<CertificationsScorer>();
builder.Services.AddSingleton<AvailabilityScorer>();
builder.Services.AddSingleton<MatchingEngine>();
builder.Services.AddScoped<IMatchingService, ScoringMatchingService>();
builder.Services.AddHostedService<ApplicationSubmittedConsumer>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var mode = builder.Configuration.GetValue<string>("InfrastructureMode") ?? "Local";
builder.Services.AddInfrastructure(builder.Configuration, mode);

var app = builder.Build();

app.UseInfrastructureMiddleware();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Service = "Matching", Timestamp = DateTime.UtcNow }));

app.Run();
