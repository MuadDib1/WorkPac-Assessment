namespace WorkPac.Recruitment.Contracts.ApiModels;

public record SubmitApplicationRequest(
    Guid CandidateId,
    string? CoverLetter,
    List<DocumentUpload>? Documents,
    Dictionary<string, string>? Answers
);

public record DocumentUpload(
    string FileName,
    string ContentType,
    string Base64Content
);
