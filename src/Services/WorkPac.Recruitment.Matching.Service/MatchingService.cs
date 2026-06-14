using Microsoft.Extensions.Logging;
using WorkPac.Recruitment.Contracts.Messaging;
using WorkPac.Recruitment.Shared.Interfaces;

namespace WorkPac.Recruitment.Matching.Service;

public class ScoringMatchingService : IMatchingService
{
    private readonly IApplicationRepository _applicationRepo;
    private readonly ICandidateRepository _candidateRepo;
    private readonly IJobPostingRepository _jobPostingRepo;
    private readonly MatchingEngine _matchingEngine;
    private readonly IEventBus _eventBus;
    private readonly ILogger<ScoringMatchingService> _logger;

    public ScoringMatchingService(
        IApplicationRepository applicationRepo,
        ICandidateRepository candidateRepo,
        IJobPostingRepository jobPostingRepo,
        MatchingEngine matchingEngine,
        IEventBus eventBus,
        ILogger<ScoringMatchingService> logger)
    {
        _applicationRepo = applicationRepo;
        _candidateRepo = candidateRepo;
        _jobPostingRepo = jobPostingRepo;
        _matchingEngine = matchingEngine;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task CalculateAndSaveMatchAsync(Guid applicationId, Guid jobPostingId, Guid candidateId, CancellationToken ct)
    {
        var candidate = await _candidateRepo.GetByIdAsync(candidateId, ct);
        var job = await _jobPostingRepo.GetByIdAsync(jobPostingId, ct);

        if (candidate is null || job is null)
        {
            _logger.LogWarning("Candidate or Job not found for matching: {AppId}", applicationId);
            return;
        }

        var matchResult = _matchingEngine.CalculateMatch(candidate, job);

        var application = await _applicationRepo.GetByIdAsync(applicationId, ct);
        if (application is not null)
        {
            application.MatchScore = (decimal)matchResult.Score;
            await _applicationRepo.UpdateAsync(application, ct);
        }

        await _eventBus.PublishAsync(new MatchCompletedEvent(
            applicationId, candidateId, jobPostingId,
            matchResult.Score,
            new MatchBreakdownData(
                matchResult.Breakdown.SkillsScore,
                matchResult.Breakdown.ExperienceScore,
                matchResult.Breakdown.LocationScore,
                matchResult.Breakdown.CertificationsScore,
                matchResult.Breakdown.AvailabilityScore
            )), ct);

        _logger.LogInformation("Match score {Score} for application {AppId}", matchResult.Score, applicationId);
    }
}
