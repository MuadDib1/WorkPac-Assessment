using WorkPac.Recruitment.Shared;
using WorkPac.Recruitment.Shared.Enums;

namespace WorkPac.Recruitment.Contracts.Messaging;

public record ApplicationStatusChangedEvent(
    Guid ApplicationId,
    ApplicationStatus FromStatus,
    ApplicationStatus ToStatus,
    string? Reason,
    Guid ChangedByUserId
) : BaseDomainEvent;
