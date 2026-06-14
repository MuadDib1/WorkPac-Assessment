using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WorkPac.Recruitment.Contracts.Messaging;
using WorkPac.Recruitment.Shared.Interfaces;

namespace WorkPac.Recruitment.Matching.Service.Consumers;

public class ApplicationSubmittedConsumer : BackgroundService
{
    private readonly IEventBus _eventBus;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ApplicationSubmittedConsumer> _logger;

    public ApplicationSubmittedConsumer(
        IEventBus eventBus,
        IServiceScopeFactory scopeFactory,
        ILogger<ApplicationSubmittedConsumer> logger)
    {
        _eventBus = eventBus;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ApplicationSubmittedConsumer started");

        await _eventBus.SubscribeAsync<ApplicationSubmittedEvent>(
            "applications.submitted",
            async message =>
            {
                _logger.LogInformation("Processing match for application {ApplicationId}", message.ApplicationId);
                using var scope = _scopeFactory.CreateScope();
                var matchingService = scope.ServiceProvider.GetRequiredService<IMatchingService>();
                await matchingService.CalculateAndSaveMatchAsync(
                    message.ApplicationId, message.JobPostingId, message.CandidateId, stoppingToken);
            },
            stoppingToken);
    }
}
