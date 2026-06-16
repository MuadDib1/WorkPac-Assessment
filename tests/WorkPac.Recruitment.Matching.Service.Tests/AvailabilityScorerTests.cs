using FluentAssertions;
using WorkPac.Recruitment.Matching.Service.Scoring;
using WorkPac.Recruitment.Shared.Domain;
using WorkPac.Recruitment.Shared.ValueObjects;

namespace WorkPac.Recruitment.Matching.Service.Tests;

public class AvailabilityScorerTests
{
    private readonly AvailabilityScorer _sut = new();

    [Fact]
    public void ActiveCandidate_ReturnsOne()
    {
        var candidate = CreateCandidate(CandidateStatus.Active);

        var score = _sut.Score(candidate);

        score.Should().Be(1.0);
    }

    [Fact]
    public void PlacedCandidate_ReturnsZero()
    {
        var candidate = CreateCandidate(CandidateStatus.Placed);

        var score = _sut.Score(candidate);

        score.Should().Be(0.0);
    }

    [Fact]
    public void UnavailableCandidate_ReturnsZero()
    {
        var candidate = CreateCandidate(CandidateStatus.Unavailable);

        var score = _sut.Score(candidate);

        score.Should().Be(0.0);
    }

    [Fact]
    public void BlacklistedCandidate_ReturnsZero()
    {
        var candidate = CreateCandidate(CandidateStatus.Blacklisted);

        var score = _sut.Score(candidate);

        score.Should().Be(0.0);
    }

    private static Candidate CreateCandidate(CandidateStatus status)
    {
        var candidate = Candidate.Create("Test", "User", "test@example.com", null,
            new Location { Suburb = "Brisbane", State = "QLD" });
        typeof(Candidate).GetProperty("Status")?.SetValue(candidate, status);
        return candidate;
    }
}
