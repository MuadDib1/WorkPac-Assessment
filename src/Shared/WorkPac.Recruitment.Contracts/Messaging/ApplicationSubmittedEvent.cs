using WorkPac.Recruitment.Shared;

namespace WorkPac.Recruitment.Contracts.Messaging;

public record ApplicationSubmittedEvent(
    Guid ApplicationId,
    Guid JobPostingId,
    Guid CandidateId,
    DateTime SubmittedAt
) : BaseDomainEvent;
