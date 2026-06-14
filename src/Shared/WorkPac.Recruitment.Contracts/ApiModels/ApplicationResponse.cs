using WorkPac.Recruitment.Shared.Enums;

namespace WorkPac.Recruitment.Contracts.ApiModels;

public record ApplicationResponse(
    Guid Id,
    Guid JobPostingId,
    Guid CandidateId,
    ApplicationStatus Status,
    string? CoverLetter,
    List<DocumentInfo> Documents,
    List<StatusHistoryEntry> StatusHistory,
    decimal? MatchScore,
    DateTime SubmittedAt,
    DateTime? UpdatedAt
);

public record DocumentInfo(
    Guid Id,
    string FileName,
    string ContentType,
    long SizeBytes,
    DateTime UploadedAt
);

public record StatusHistoryEntry(
    ApplicationStatus FromStatus,
    ApplicationStatus ToStatus,
    string ChangedBy,
    string? Reason,
    DateTime ChangedAt
);
