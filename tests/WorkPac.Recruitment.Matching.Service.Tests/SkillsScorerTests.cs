using FluentAssertions;
using WorkPac.Recruitment.Matching.Service.Scoring;

namespace WorkPac.Recruitment.Matching.Service.Tests;

public class SkillsScorerTests
{
    private readonly SkillsScorer _sut = new();

    [Fact]
    public void AllSkillsMatch_ReturnsOne()
    {
        var score = _sut.Score(["a", "b", "c"], ["a", "b", "c"]);
        score.Should().Be(1.0);
    }

    [Fact]
    public void NoSkillsMatch_ReturnsZero()
    {
        var score = _sut.Score(["x", "y"], ["a", "b"]);
        score.Should().Be(0.0);
    }

    [Fact]
    public void PartialMatch_ReturnsJaccardSimilarity()
    {
        var score = _sut.Score(["a", "b", "c"], ["a", "b"]);
        score.Should().BeApproximately(0.666, 0.01);
    }

    [Fact]
    public void NoRequiredSkills_ReturnsOne()
    {
        var score = _sut.Score(["a", "b"], []);
        score.Should().Be(1.0);
    }

    [Fact]
    public void NoCandidateSkills_ReturnsZero()
    {
        var score = _sut.Score([], ["a", "b"]);
        score.Should().Be(0.0);
    }

    [Fact]
    public void CaseInsensitive_ReturnsMatch()
    {
        var score = _sut.Score(["Diesel Fitting"], ["diesel fitting"]);
        score.Should().Be(1.0);
    }
}
