using WorkPac.Recruitment.Shared.Domain;

namespace WorkPac.Recruitment.Shared.Interfaces;

public interface ICandidateRepository
{
    Task<Candidate?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Candidate>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(Candidate candidate, CancellationToken ct = default);
}
