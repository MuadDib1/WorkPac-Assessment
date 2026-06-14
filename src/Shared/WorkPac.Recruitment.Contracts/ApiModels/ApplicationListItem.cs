using WorkPac.Recruitment.Shared.Enums;

namespace WorkPac.Recruitment.Contracts.ApiModels;

public record ApplicationListItem(
    Guid Id,
    Guid JobPostingId,
    string JobTitle,
    Guid CandidateId,
    string CandidateName,
    ApplicationStatus Status,
    decimal? MatchScore,
    DateTime SubmittedAt
);
