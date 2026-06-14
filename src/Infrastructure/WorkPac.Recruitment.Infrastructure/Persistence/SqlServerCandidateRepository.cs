using Microsoft.EntityFrameworkCore;
using WorkPac.Recruitment.Shared.Domain;
using WorkPac.Recruitment.Shared.Interfaces;

namespace WorkPac.Recruitment.Infrastructure.Persistence;

public class SqlServerCandidateRepository : ICandidateRepository
{
    private readonly RecruitmentDbContext _context;

    public SqlServerCandidateRepository(RecruitmentDbContext context)
    {
        _context = context;
    }

    public async Task<List<Candidate>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Candidates.ToListAsync(ct);
    }

    public async Task<Candidate?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Candidates.FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task AddAsync(Candidate candidate, CancellationToken ct = default)
    {
        await _context.Candidates.AddAsync(candidate, ct);
        await _context.SaveChangesAsync(ct);
    }
}
