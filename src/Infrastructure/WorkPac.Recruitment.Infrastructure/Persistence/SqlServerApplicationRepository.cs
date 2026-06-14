using Microsoft.EntityFrameworkCore;
using WorkPac.Recruitment.Shared.Domain;
using WorkPac.Recruitment.Shared.Enums;
using WorkPac.Recruitment.Shared.Interfaces;

namespace WorkPac.Recruitment.Infrastructure.Persistence;

public class SqlServerApplicationRepository : IApplicationRepository
{
    private readonly RecruitmentDbContext _context;

    public SqlServerApplicationRepository(RecruitmentDbContext context)
    {
        _context = context;
    }

    public async Task<Application?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Applications
            .Include(a => a.StatusHistory)
            .FirstOrDefaultAsync(a => a.Id == id, ct);
    }

    public async Task<List<Application>> GetByJobPostingAsync(Guid jobPostingId, CancellationToken ct = default)
    {
        return await _context.Applications
            .Where(a => a.JobPostingId == jobPostingId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<List<Application>> GetByCandidateAsync(Guid candidateId, CancellationToken ct = default)
    {
        return await _context.Applications
            .Where(a => a.CandidateId == candidateId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsAsync(Guid jobPostingId, Guid candidateId, CancellationToken ct = default)
    {
        return await _context.Applications
            .AnyAsync(a => a.JobPostingId == jobPostingId && a.CandidateId == candidateId, ct);
    }

    public async Task AddAsync(Application application, CancellationToken ct = default)
    {
        await _context.Applications.AddAsync(application, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Application application, CancellationToken ct = default)
    {
        _context.Applications.Update(application);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<List<Application>> GetByStatusAsync(ApplicationStatus status, CancellationToken ct = default)
    {
        return await _context.Applications
            .Where(a => a.Status == status)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(ct);
    }
}
