using FluentAssertions;
using WorkPac.Recruitment.Matching.Service.Scoring;
using WorkPac.Recruitment.Shared.Domain;
using WorkPac.Recruitment.Shared.Enums;
using WorkPac.Recruitment.Shared.ValueObjects;

namespace WorkPac.Recruitment.Matching.Service.Tests;

public class MatchingEngineTests
{
    private readonly MatchingEngine _sut;

    public MatchingEngineTests()
    {
        _sut = new MatchingEngine(
            new SkillsScorer(),
            new ExperienceScorer(),
            new LocationScorer(),
            new CertificationsScorer(),
            new AvailabilityScorer());
    }

    [Fact]
    public void PerfectMatch_ReturnsMaxScore()
    {
        var candidate = CreatePerfectCandidate();
        var job = CreatePerfectJob();

        var result = _sut.CalculateMatch(candidate, job);

        result.Score.Should().BeApproximately(1.0, 0.01);
    }

    [Fact]
    public void NoSkillsMatch_ReturnsLowScore()
    {
        var candidate = CreateCandidate(skills: ["cooking"], certs: []);
        var job = CreatePerfectJob();

        var result = _sut.CalculateMatch(candidate, job);

        result.Score.Should().BeLessThan(0.7);
        result.Breakdown.SkillsScore.Should().Be(0.0);
    }

    [Fact]
    public void NoExperience_ReturnsReducedScore()
    {
        var candidate = CreateCandidate(experienceYears: 0);
        var job = CreatePerfectJob();

        var result = _sut.CalculateMatch(candidate, job);

        result.Breakdown.ExperienceScore.Should().Be(0.0);
    }

    [Fact]
    public void PartialExperience_ReturnsProportionalScore()
    {
        var candidate = CreateCandidate(experienceYears: 2);
        var job = CreatePerfectJob(requiredExperience: 5);

        var result = _sut.CalculateMatch(candidate, job);

        result.Breakdown.ExperienceScore.Should().BeApproximately(0.4, 0.01);
    }

    [Fact]
    public void SameSiteLocation_GetsMaxLocationScore()
    {
        var candidate = CreateCandidate(location: new Location { SiteName = "Peak Downs" });
        var job = CreatePerfectJob(location: new Location { SiteName = "Peak Downs" });

        var result = _sut.CalculateMatch(candidate, job);

        result.Breakdown.LocationScore.Should().Be(1.0);
    }

    [Fact]
    public void FifoWilling_GetsHighLocationScore()
    {
        var candidate = CreateCandidate(location: new Location { State = "QLD" }, fifoWilling: true);
        var job = CreatePerfectJob(location: new Location { SiteName = "Peak Downs" });

        var result = _sut.CalculateMatch(candidate, job);

        result.Breakdown.LocationScore.Should().Be(0.8);
    }

    [Fact]
    public void SameState_GetsHalfLocationScore()
    {
        var candidate = CreateCandidate(location: new Location { State = "QLD" });
        var job = CreatePerfectJob(location: new Location { State = "QLD" });

        var result = _sut.CalculateMatch(candidate, job);

        result.Breakdown.LocationScore.Should().Be(0.5);
    }

    [Fact]
    public void ActiveCandidate_GetsMaxAvailabilityScore()
    {
        var candidate = CreateCandidate();
        var job = CreatePerfectJob();

        var result = _sut.CalculateMatch(candidate, job);

        result.Breakdown.AvailabilityScore.Should().Be(1.0);
    }

    [Fact]
    public void PlacedCandidate_GetsZeroAvailabilityScore()
    {
        var candidate = CreateCandidate(status: CandidateStatus.Placed);
        var job = CreatePerfectJob();

        var result = _sut.CalculateMatch(candidate, job);

        result.Breakdown.AvailabilityScore.Should().Be(0.0);
    }

    [Fact]
    public void AllCertsMatch_GetsMaxCertScore()
    {
        var candidate = CreateCandidate(certs: ["Standard 11", "RIISS", "Coal Board Medical"]);
        var job = CreatePerfectJob(requiredCerts: ["Standard 11", "RIISS", "Coal Board Medical"]);

        var result = _sut.CalculateMatch(candidate, job);

        result.Breakdown.CertificationsScore.Should().Be(1.0);
    }

    [Fact]
    public void HalfCertsMatch_GetsHalfCertScore()
    {
        var candidate = CreateCandidate(certs: ["Standard 11"]);
        var job = CreatePerfectJob(requiredCerts: ["Standard 11", "RIISS"]);

        var result = _sut.CalculateMatch(candidate, job);

        result.Breakdown.CertificationsScore.Should().Be(0.5);
    }

    [Fact]
    public void NoRequiredCerts_GetsMaxCertScore()
    {
        var candidate = CreateCandidate(certs: []);
        var job = CreatePerfectJob(requiredCerts: []);

        var result = _sut.CalculateMatch(candidate, job);

        result.Breakdown.CertificationsScore.Should().Be(1.0);
    }

    private static Candidate CreatePerfectCandidate() =>
        CreateCandidate(
            skills: ["diesel fitting", "hydraulics", "welding"],
            certs: ["Standard 11", "RIISS", "Coal Board Medical"],
            experienceYears: 5,
            location: new Location { SiteName = "Peak Downs", State = "QLD" },
            fifoWilling: true);

    private static JobPosting CreatePerfectJob(int requiredExperience = 5, List<string>? requiredCerts = null, Location? location = null)
    {
        var job = JobPosting.Create(
            Guid.NewGuid(), "Diesel Fitter", "Heavy diesel fitting", JobCategory.Mining,
            location ?? new Location { SiteName = "Peak Downs", State = "QLD" },
            new PayRate { Amount = 65, Currency = "AUD", Interval = RateInterval.Hourly },
            EmploymentType.Contract, requiredExperience, Guid.NewGuid());

        typeof(JobPosting).GetProperty("RequiredSkills")?.SetValue(job, new List<string> { "diesel fitting", "hydraulics", "welding" });
        typeof(JobPosting).GetProperty("RequiredCertifications")?.SetValue(job, requiredCerts ?? []);
        return job;
    }

    private static Candidate CreateCandidate(
        List<string>? skills = null, List<string>? certs = null,
        int experienceYears = 5, Location? location = null,
        bool fifoWilling = false, CandidateStatus status = CandidateStatus.Active)
    {
        var candidate = Candidate.Create(
            "John", "Smith", "john@example.com", null,
            location ?? new Location { Suburb = "Brisbane", State = "QLD" });

        typeof(Candidate).GetProperty("Skills")?.SetValue(candidate, skills ?? []);
        typeof(Candidate).GetProperty("Certifications")?.SetValue(candidate, certs ?? []);
        typeof(Candidate).GetProperty("TotalExperienceYears")?.SetValue(candidate, experienceYears);
        typeof(Candidate).GetProperty("FifoWilling")?.SetValue(candidate, fifoWilling);
        typeof(Candidate).GetProperty("Status")?.SetValue(candidate, status);

        return candidate;
    }
}
