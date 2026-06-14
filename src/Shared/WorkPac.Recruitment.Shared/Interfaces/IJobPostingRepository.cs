using WorkPac.Recruitment.Shared.Domain;

namespace WorkPac.Recruitment.Shared.Interfaces;

public interface IJobPostingRepository
{
    Task<JobPosting?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<JobPosting>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(JobPosting jobPosting, CancellationToken ct = default);
    Task UpdateAsync(JobPosting jobPosting, CancellationToken ct = default);
}
