using FluentAssertions;
using WorkPac.Recruitment.Matching.Service.Scoring;

namespace WorkPac.Recruitment.Matching.Service.Tests;

public class CertificationsScorerTests
{
    private readonly CertificationsScorer _sut = new();

    [Fact]
    public void NoRequiredCerts_ReturnsOne()
    {
        var score = _sut.Score(["Standard 11"], []);
        score.Should().Be(1.0);
    }

    [Fact]
    public void NoCandidateCerts_ReturnsZero()
    {
        var score = _sut.Score([], ["Standard 11"]);
        score.Should().Be(0.0);
    }

    [Fact]
    public void AllRequiredCertsMatch_ReturnsOne()
    {
        var score = _sut.Score(["Standard 11", "RIISS"], ["Standard 11", "RIISS"]);
        score.Should().Be(1.0);
    }

    [Fact]
    public void HalfCertsMatch_ReturnsHalf()
    {
        var score = _sut.Score(["Standard 11"], ["Standard 11", "RIISS"]);
        score.Should().Be(0.5);
    }

    [Fact]
    public void CandidateHasExtraCerts_StillReturnsOne()
    {
        var score = _sut.Score(
            ["Standard 11", "RIISS", "Coal Board Medical"],
            ["Standard 11", "RIISS"]);
        score.Should().Be(1.0);
    }

    [Fact]
    public void CaseInsensitive_ReturnsMatch()
    {
        var score = _sut.Score(["standard 11"], ["Standard 11"]);
        score.Should().Be(1.0);
    }

    [Fact]
    public void NoMatch_ReturnsZero()
    {
        var score = _sut.Score(["Standard 11"], ["RIISS", "Coal Board Medical"]);
        score.Should().Be(0.0);
    }
}
