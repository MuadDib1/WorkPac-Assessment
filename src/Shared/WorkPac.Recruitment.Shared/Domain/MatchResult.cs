namespace WorkPac.Recruitment.Shared.Domain;

public class MatchResult : BaseEntity
{
    public Guid CandidateId { get; private set; }
    public Guid JobPostingId { get; private set; }
    public double Score { get; private set; }
    public MatchBreakdown Breakdown { get; private set; } = new();
    public DateTime? ViewedAt { get; set; }

    private MatchResult() { }

    public MatchResult(Guid candidateId, Guid jobPostingId, double score, MatchBreakdown breakdown)
    {
        CandidateId = candidateId;
        JobPostingId = jobPostingId;
        Score = Math.Clamp(score, 0.0, 1.0);
        Breakdown = breakdown;
    }
}

public record MatchBreakdown
{
    public double SkillsScore { get; init; }
    public double ExperienceScore { get; init; }
    public double LocationScore { get; init; }
    public double CertificationsScore { get; init; }
    public double AvailabilityScore { get; init; }
}
