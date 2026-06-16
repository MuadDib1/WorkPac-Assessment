using FluentAssertions;
using WorkPac.Recruitment.Matching.Service.Scoring;
using WorkPac.Recruitment.Shared.Domain;
using WorkPac.Recruitment.Shared.Enums;
using WorkPac.Recruitment.Shared.ValueObjects;

namespace WorkPac.Recruitment.Matching.Service.Tests;

public class LocationScorerTests
{
    private readonly LocationScorer _sut = new();

    [Fact]
    public void NoJobLocation_ReturnsOne()
    {
        var candidate = CreateCandidate(location: new Location());
        var job = CreateJob(location: new Location());

        var score = _sut.Score(candidate, job);

        score.Should().Be(1.0);
    }

    [Fact]
    public void SameSite_ReturnsOne()
    {
        var candidate = CreateCandidate(location: new Location { SiteName = "Peak Downs" });
        var job = CreateJob(location: new Location { SiteName = "Peak Downs" });

        var score = _sut.Score(candidate, job);

        score.Should().Be(1.0);
    }

    [Fact]
    public void DifferentSite_FifoWilling_ReturnsEight()
    {
        var candidate = CreateCandidate(location: new Location { SiteName = "Goonyella" }, fifoWilling: true);
        var job = CreateJob(location: new Location { SiteName = "Peak Downs" });

        var score = _sut.Score(candidate, job);

        score.Should().Be(0.8);
    }

    [Fact]
    public void DifferentSite_WillingToRelocate_ReturnsEight()
    {
        var candidate = CreateCandidate(location: new Location { SiteName = "Goonyella" }, relocateWilling: true);
        var job = CreateJob(location: new Location { SiteName = "Peak Downs" });

        var score = _sut.Score(candidate, job);

        score.Should().Be(0.8);
    }

    [Fact]
    public void SameState_ReturnsHalf()
    {
        var candidate = CreateCandidate(location: new Location { State = "QLD" });
        var job = CreateJob(location: new Location { State = "QLD" });

        var score = _sut.Score(candidate, job);

        score.Should().Be(0.5);
    }

    [Fact]
    public void DifferentSite_NotFifo_ReturnsZero()
    {
        var candidate = CreateCandidate(location: new Location { SiteName = "Goonyella" });
        var job = CreateJob(location: new Location { SiteName = "Peak Downs" });

        var score = _sut.Score(candidate, job);

        score.Should().Be(0.0);
    }

    [Fact]
    public void DifferentState_ReturnsZero()
    {
        var candidate = CreateCandidate(location: new Location { State = "NSW" });
        var job = CreateJob(location: new Location { State = "QLD" });

        var score = _sut.Score(candidate, job);

        score.Should().Be(0.0);
    }

    [Fact]
    public void SiteName_CaseInsensitive_ReturnsOne()
    {
        var candidate = CreateCandidate(location: new Location { SiteName = "peak downs" });
        var job = CreateJob(location: new Location { SiteName = "Peak Downs" });

        var score = _sut.Score(candidate, job);

        score.Should().Be(1.0);
    }

    private static Candidate CreateCandidate(Location location, bool fifoWilling = false, bool relocateWilling = false)
    {
        var candidate = Candidate.Create("Test", "User", "test@example.com", null, location);
        typeof(Candidate).GetProperty("FifoWilling")?.SetValue(candidate, fifoWilling);
        typeof(Candidate).GetProperty("WillingToRelocate")?.SetValue(candidate, relocateWilling);
        return candidate;
    }

    private static JobPosting CreateJob(Location location)
    {
        return JobPosting.Create(
            Guid.NewGuid(), "Test Role", "Description", JobCategory.Mining,
            location,
            new PayRate { Amount = 50, Currency = "AUD", Interval = RateInterval.Hourly },
            EmploymentType.Contract, 3, Guid.NewGuid());
    }
}
