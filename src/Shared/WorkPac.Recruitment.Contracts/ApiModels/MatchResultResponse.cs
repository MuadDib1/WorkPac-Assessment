namespace WorkPac.Recruitment.Contracts.ApiModels;

public record MatchResultResponse(
    Guid ApplicationId,
    Guid CandidateId,
    Guid JobPostingId,
    double Score,
    MatchBreakdown Breakdown,
    DateTime CalculatedAt
);

public record MatchBreakdown(
    double SkillsScore,
    double ExperienceScore,
    double LocationScore,
    double CertificationsScore,
    double AvailabilityScore
);
