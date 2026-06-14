namespace WorkPac.Recruitment.Contracts.ApiModels;

public record InitiateComplianceCheckRequest(
    List<string> CheckTypes
);

public record ComplianceCheckResponse(
    Guid Id,
    Guid ApplicationId,
    string Status,
    List<ComplianceCheckItem> Checks
);

public record ComplianceCheckItem(
    string Type,
    string Status,
    DateTime? VerifiedAt
);
