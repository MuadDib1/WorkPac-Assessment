using FluentAssertions;
using WorkPac.Recruitment.Matching.Service.Scoring;

namespace WorkPac.Recruitment.Matching.Service.Tests;

public class ExperienceScorerTests
{
    private readonly ExperienceScorer _sut = new();

    [Fact]
    public void NoRequiredExperience_ReturnsOne()
    {
        var score = _sut.Score(5, 0);
        score.Should().Be(1.0);
    }

    [Fact]
    public void NoCandidateExperience_ReturnsZero()
    {
        var score = _sut.Score(0, 5);
        score.Should().Be(0.0);
    }

    [Fact]
    public void CandidateYearsEqualsRequired_ReturnsOne()
    {
        var score = _sut.Score(5, 5);
        score.Should().Be(1.0);
    }

    [Fact]
    public void CandidateYearsExceedsRequired_CapsAtOne()
    {
        var score = _sut.Score(10, 5);
        score.Should().Be(1.0);
    }

    [Fact]
    public void PartialExperience_ReturnsProportionalScore()
    {
        var score = _sut.Score(3, 10);
        score.Should().Be(0.3);
    }

    [Fact]
    public void BothZero_ReturnsOne()
    {
        var score = _sut.Score(0, 0);
        score.Should().Be(1.0);
    }
}
