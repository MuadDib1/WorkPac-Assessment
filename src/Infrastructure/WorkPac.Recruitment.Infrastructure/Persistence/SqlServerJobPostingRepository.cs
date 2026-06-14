using Microsoft.EntityFrameworkCore;
using WorkPac.Recruitment.Shared.Domain;
using WorkPac.Recruitment.Shared.Interfaces;

namespace WorkPac.Recruitment.Infrastructure.Persistence;

public class SqlServerJobPostingRepository : IJobPostingRepository
{
    private readonly RecruitmentDbContext _context;

    public SqlServerJobPostingRepository(RecruitmentDbContext context)
    {
        _context = context;
    }

    public async Task<List<JobPosting>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.JobPostings.ToListAsync(ct);
    }

    public async Task<JobPosting?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.JobPostings.FirstOrDefaultAsync(j => j.Id == id, ct);
    }

    public async Task AddAsync(JobPosting jobPosting, CancellationToken ct = default)
    {
        await _context.JobPostings.AddAsync(jobPosting, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(JobPosting jobPosting, CancellationToken ct = default)
    {
        _context.JobPostings.Update(jobPosting);
        await _context.SaveChangesAsync(ct);
    }
}
