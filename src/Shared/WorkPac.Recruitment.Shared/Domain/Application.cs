using WorkPac.Recruitment.Shared.Enums;
using WorkPac.Recruitment.Shared.Exceptions;
using WorkPac.Recruitment.Shared.ValueObjects;

namespace WorkPac.Recruitment.Shared.Domain;

public class Application : BaseEntity
{
    public Guid JobPostingId { get; private set; }
    public Guid CandidateId { get; private set; }
    public ApplicationStatus Status { get; private set; } = ApplicationStatus.Draft;
    public string? CoverLetter { get; private set; }
    public List<DocumentReference> Documents { get; private set; } = [];
    public List<StatusHistoryEntry> StatusHistory { get; private set; } = [];
    public decimal? MatchScore { get; set; }

    private Application() { }

    public static Application Submit(Guid jobPostingId, Guid candidateId, string? coverLetter)
    {
        var application = new Application
        {
            JobPostingId = jobPostingId,
            CandidateId = candidateId,
            CoverLetter = coverLetter,
            Status = ApplicationStatus.Submitted
        };

        application.StatusHistory.Add(new StatusHistoryEntry(
            ApplicationStatus.Draft,
            ApplicationStatus.Submitted,
            candidateId.ToString(),
            null,
            DateTime.UtcNow));

        return application;
    }

    public void UpdateStatus(ApplicationStatus newStatus, Guid changedByUserId, string? reason)
    {
        var validTransitions = GetAllowedTransitions(Status);
        if (!validTransitions.Contains(newStatus))
            throw new InvalidStatusTransitionException(Status.ToString(), newStatus.ToString());

        var oldStatus = Status;
        Status = newStatus;
        StatusHistory.Add(new StatusHistoryEntry(
            oldStatus, newStatus, changedByUserId.ToString(), reason, DateTime.UtcNow));
    }

    public void AddDocument(DocumentReference document)
    {
        Documents.Add(document);
    }

    public void Withdraw()
    {
        if (Status is ApplicationStatus.Placed or ApplicationStatus.Rejected or ApplicationStatus.Withdrawn)
            throw new InvalidStatusTransitionException(Status.ToString(), ApplicationStatus.Withdrawn.ToString());

        var oldStatus = Status;
        Status = ApplicationStatus.Withdrawn;
        StatusHistory.Add(new StatusHistoryEntry(
            oldStatus, ApplicationStatus.Withdrawn, CandidateId.ToString(), "Candidate withdrew application", DateTime.UtcNow));
    }

    private static HashSet<ApplicationStatus> GetAllowedTransitions(ApplicationStatus currentStatus) => currentStatus switch
    {
        ApplicationStatus.Draft => [ApplicationStatus.Submitted, ApplicationStatus.Withdrawn],
        ApplicationStatus.Submitted => [ApplicationStatus.UnderReview, ApplicationStatus.Rejected, ApplicationStatus.Withdrawn, ApplicationStatus.OnHold],
        ApplicationStatus.UnderReview => [ApplicationStatus.Shortlisted, ApplicationStatus.Rejected, ApplicationStatus.Withdrawn, ApplicationStatus.OnHold],
        ApplicationStatus.Shortlisted => [ApplicationStatus.InterviewScheduled, ApplicationStatus.Rejected, ApplicationStatus.Withdrawn],
        ApplicationStatus.InterviewScheduled => [ApplicationStatus.Interviewed, ApplicationStatus.Rejected, ApplicationStatus.Withdrawn],
        ApplicationStatus.Interviewed => [ApplicationStatus.Offered, ApplicationStatus.Rejected, ApplicationStatus.Withdrawn],
        ApplicationStatus.Offered => [ApplicationStatus.Accepted, ApplicationStatus.Rejected, ApplicationStatus.Withdrawn],
        ApplicationStatus.Accepted => [ApplicationStatus.Mobilising, ApplicationStatus.Rejected],
        ApplicationStatus.Mobilising => [ApplicationStatus.Placed, ApplicationStatus.Rejected],
        ApplicationStatus.OnHold => [ApplicationStatus.UnderReview, ApplicationStatus.Rejected, ApplicationStatus.Withdrawn],
        _ => []
    };
}

public record StatusHistoryEntry(
    ApplicationStatus FromStatus,
    ApplicationStatus ToStatus,
    string ChangedBy,
    string? Reason,
    DateTime ChangedAt
);
