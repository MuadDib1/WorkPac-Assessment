namespace WorkPac.Recruitment.Contracts.ApiModels;

public record DocumentListItem(
    Guid Id,
    string FileName,
    string ContentType,
    long SizeBytes,
    DateTime UploadedAt,
    Guid ApplicationId,
    Guid CandidateId,
    string CandidateName
);
