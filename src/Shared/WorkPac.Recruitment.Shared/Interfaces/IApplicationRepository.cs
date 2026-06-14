using WorkPac.Recruitment.Shared.Domain;
using WorkPac.Recruitment.Shared.Enums;

namespace WorkPac.Recruitment.Shared.Interfaces;

public interface IApplicationRepository
{
    Task<Application?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Application>> GetByJobPostingAsync(Guid jobPostingId, CancellationToken ct = default);
    Task<List<Application>> GetByCandidateAsync(Guid candidateId, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid jobPostingId, Guid candidateId, CancellationToken ct = default);
    Task AddAsync(Application application, CancellationToken ct = default);
    Task UpdateAsync(Application application, CancellationToken ct = default);
    Task<List<Application>> GetByStatusAsync(ApplicationStatus status, CancellationToken ct = default);
}
