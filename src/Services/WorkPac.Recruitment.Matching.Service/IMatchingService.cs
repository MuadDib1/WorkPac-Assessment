namespace WorkPac.Recruitment.Matching.Service;

public interface IMatchingService
{
    Task CalculateAndSaveMatchAsync(Guid applicationId, Guid jobPostingId, Guid candidateId, CancellationToken ct);
}
