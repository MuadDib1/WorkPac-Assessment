using WorkPac.Recruitment.Shared;

namespace WorkPac.Recruitment.Contracts.Messaging;

public record MatchCompletedEvent(
    Guid ApplicationId,
    Guid CandidateId,
    Guid JobPostingId,
    double Score,
    MatchBreakdownData Breakdown
) : BaseDomainEvent;

public record MatchBreakdownData(
    double SkillsScore,
    double ExperienceScore,
    double LocationScore,
    double CertificationsScore,
    double AvailabilityScore
);
