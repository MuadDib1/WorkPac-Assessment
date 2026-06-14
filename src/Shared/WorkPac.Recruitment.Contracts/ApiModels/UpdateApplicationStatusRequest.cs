using WorkPac.Recruitment.Shared.Enums;

namespace WorkPac.Recruitment.Contracts.ApiModels;

public record UpdateApplicationStatusRequest(
    ApplicationStatus NewStatus,
    string? Reason,
    Guid ChangedByUserId
);
