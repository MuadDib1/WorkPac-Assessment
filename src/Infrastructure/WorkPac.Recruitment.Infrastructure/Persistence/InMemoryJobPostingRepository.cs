using System.Collections.Concurrent;
using WorkPac.Recruitment.Shared.Domain;
using WorkPac.Recruitment.Shared.Interfaces;

namespace WorkPac.Recruitment.Infrastructure.Persistence;

public class InMemoryJobPostingRepository : IJobPostingRepository
{
    private readonly ConcurrentDictionary<Guid, JobPosting> _store = new();

    public Task<List<JobPosting>> GetAllAsync(CancellationToken ct = default)
    {
        return Task.FromResult(_store.Values.ToList());
    }

    public Task<JobPosting?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        _store.TryGetValue(id, out var job);
        return Task.FromResult(job);
    }

    public Task AddAsync(JobPosting jobPosting, CancellationToken ct = default)
    {
        _store.TryAdd(jobPosting.Id, jobPosting);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(JobPosting jobPosting, CancellationToken ct = default)
    {
        _store.TryUpdate(jobPosting.Id, jobPosting, _store.GetValueOrDefault(jobPosting.Id)!);
        return Task.CompletedTask;
    }
}
