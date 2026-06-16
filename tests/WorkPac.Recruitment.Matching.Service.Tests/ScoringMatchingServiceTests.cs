using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using WorkPac.Recruitment.Contracts.Messaging;
using WorkPac.Recruitment.Matching.Service.Scoring;
using WorkPac.Recruitment.Shared.Domain;
using WorkPac.Recruitment.Shared.Enums;
using WorkPac.Recruitment.Shared.Interfaces;
using WorkPac.Recruitment.Shared.ValueObjects;

namespace WorkPac.Recruitment.Matching.Service.Tests;

public class ScoringMatchingServiceTests
{
    private readonly IApplicationRepository _appRepo;
    private readonly ICandidateRepository _candidateRepo;
    private readonly IJobPostingRepository _jobRepo;
    private readonly IEventBus _eventBus;
    private readonly MatchingEngine _engine;
    private readonly ScoringMatchingService _sut;

    public ScoringMatchingServiceTests()
    {
        _appRepo = Substitute.For<IApplicationRepository>();
        _candidateRepo = Substitute.For<ICandidateRepository>();
        _jobRepo = Substitute.For<IJobPostingRepository>();
        _eventBus = Substitute.For<IEventBus>();
        _engine = new MatchingEngine(
            new SkillsScorer(),
            new ExperienceScorer(),
            new LocationScorer(),
            new CertificationsScorer(),
            new AvailabilityScorer());

        _sut = new ScoringMatchingService(
            _appRepo,
            _candidateRepo,
            _jobRepo,
            _engine,
            _eventBus,
            Substitute.For<ILogger<ScoringMatchingService>>());
    }

    [Fact]
    public async Task CalculateAndSaveMatchAsync_HappyPath_SavesScoreAndPublishesEvent()
    {
        var appId = Guid.NewGuid();
        var jobId = Guid.NewGuid();
        var candidateId = Guid.NewGuid();
        var application = CreateApplication(appId, jobId, candidateId);
        var candidate = CreatePerfectCandidate(candidateId);
        var job = CreatePerfectJob(jobId);

        _appRepo.GetByIdAsync(appId, default).Returns(application);
        _candidateRepo.GetByIdAsync(candidateId, default).Returns(candidate);
        _jobRepo.GetByIdAsync(jobId, default).Returns(job);

        await _sut.CalculateAndSaveMatchAsync(appId, jobId, candidateId, default);

        application.MatchScore.Should().BeGreaterThan(0);
        await _appRepo.Received(1).UpdateAsync(application, default);
        await _eventBus.Received(1).PublishAsync(
            Arg.Is<MatchCompletedEvent>(e =>
                e.ApplicationId == appId &&
                e.CandidateId == candidateId &&
                e.JobPostingId == jobId &&
                e.Score > 0),
            default);
    }

    [Fact]
    public async Task CalculateAndSaveMatchAsync_CandidateNotFound_DoesNotCrash()
    {
        var appId = Guid.NewGuid();
        _appRepo.GetByIdAsync(appId, default).Returns((Application?)null);
        _candidateRepo.GetByIdAsync(Arg.Any<Guid>(), default).Returns((Candidate?)null);
        _jobRepo.GetByIdAsync(Arg.Any<Guid>(), default).Returns(CreatePerfectJob(Guid.NewGuid()));

        await _sut.CalculateAndSaveMatchAsync(appId, Guid.NewGuid(), Guid.NewGuid(), default);

        await _appRepo.DidNotReceive().UpdateAsync(Arg.Any<Application>(), default);
        await _eventBus.DidNotReceive().PublishAsync(Arg.Any<MatchCompletedEvent>(), default);
    }

    [Fact]
    public async Task CalculateAndSaveMatchAsync_JobNotFound_DoesNotCrash()
    {
        var appId = Guid.NewGuid();
        var candidateId = Guid.NewGuid();
        _appRepo.GetByIdAsync(appId, default).Returns((Application?)null);
        _candidateRepo.GetByIdAsync(candidateId, default).Returns(CreatePerfectCandidate(candidateId));
        _jobRepo.GetByIdAsync(Arg.Any<Guid>(), default).Returns((JobPosting?)null);

        await _sut.CalculateAndSaveMatchAsync(appId, Guid.NewGuid(), candidateId, default);

        await _appRepo.DidNotReceive().UpdateAsync(Arg.Any<Application>(), default);
        await _eventBus.DidNotReceive().PublishAsync(Arg.Any<MatchCompletedEvent>(), default);
    }

    private static Application CreateApplication(Guid id, Guid jobId, Guid candidateId)
    {
        var app = Application.Submit(jobId, candidateId, "Cover letter");
        typeof(Application).GetProperty("Id")?.SetValue(app, id);
        return app!;
    }

    private static Candidate CreatePerfectCandidate(Guid id)
    {
        var candidate = Candidate.Create("John", "Smith", "john@example.com", null,
            new Location { SiteName = "Peak Downs", State = "QLD" });
        typeof(Candidate).GetProperty("Id")?.SetValue(candidate, id);
        typeof(Candidate).GetProperty("Skills")?.SetValue(candidate, new List<string> { "diesel fitting", "hydraulics" });
        typeof(Candidate).GetProperty("Certifications")?.SetValue(candidate, new List<string>());
        typeof(Candidate).GetProperty("TotalExperienceYears")?.SetValue(candidate, 5);
        typeof(Candidate).GetProperty("Status")?.SetValue(candidate, CandidateStatus.Active);
        typeof(Candidate).GetProperty("FifoWilling")?.SetValue(candidate, true);
        return candidate;
    }

    private static JobPosting CreatePerfectJob(Guid id)
    {
        var job = JobPosting.Create(
            id, "Diesel Fitter", "Heavy diesel fitting", JobCategory.Mining,
            new Location { SiteName = "Peak Downs", State = "QLD" },
            new PayRate { Amount = 65, Currency = "AUD", Interval = RateInterval.Hourly },
            EmploymentType.Contract, 5, Guid.NewGuid());
        typeof(JobPosting).GetProperty("RequiredSkills")?.SetValue(job, new List<string> { "diesel fitting", "hydraulics" });
        typeof(JobPosting).GetProperty("RequiredCertifications")?.SetValue(job, new List<string>());
        return job;
    }
}
