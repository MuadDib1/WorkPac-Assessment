using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WorkPac.Recruitment.Infrastructure.Data;
using WorkPac.Recruitment.Infrastructure.Messaging;
using WorkPac.Recruitment.Infrastructure.Middleware;
using WorkPac.Recruitment.Infrastructure.Persistence;
using WorkPac.Recruitment.Infrastructure.Storage;
using WorkPac.Recruitment.Shared.Interfaces;

namespace WorkPac.Recruitment.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        string infrastructureMode = "Local")
    {
        switch (infrastructureMode)
        {
            case "Azure":
                services.AddDbContext<RecruitmentDbContext>(options =>
                    options.UseSqlServer(configuration.GetConnectionString("Database")));
                services.AddScoped<IApplicationRepository, SqlServerApplicationRepository>();
                services.AddScoped<IJobPostingRepository, SqlServerJobPostingRepository>();
                services.AddScoped<ICandidateRepository, SqlServerCandidateRepository>();
                services.AddSingleton<IEventBus, RabbitMqEventBus>();
                services.Configure<RabbitMqOptions>(configuration.GetSection("RabbitMq"));
                break;

            case "RabbitMQ":
                services.AddSingleton<IApplicationRepository, InMemoryApplicationRepository>();
                services.AddSingleton<IJobPostingRepository, InMemoryJobPostingRepository>();
                services.AddSingleton<ICandidateRepository, InMemoryCandidateRepository>();
                services.AddSingleton<IEventBus, RabbitMqEventBus>();
                services.Configure<RabbitMqOptions>(configuration.GetSection("RabbitMq"));
                break;

            default: // "Local"
                services.AddSingleton<IApplicationRepository, InMemoryApplicationRepository>();
                services.AddSingleton<IJobPostingRepository, InMemoryJobPostingRepository>();
                services.AddSingleton<ICandidateRepository, InMemoryCandidateRepository>();
                services.AddSingleton<IEventBus, InMemoryEventBus>();
                break;
        }

        services.AddScoped<DataSeeder>();

        services.AddSingleton<IBlobStorage>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<LocalFileBlobStorage>>();
            return new LocalFileBlobStorage(Path.GetTempPath(), logger);
        });

        return services;
    }

    public static WebApplication UseInfrastructureMiddleware(this WebApplication app)
    {
        app.UseMiddleware<ErrorHandlingMiddleware>();
        return app;
    }
}
