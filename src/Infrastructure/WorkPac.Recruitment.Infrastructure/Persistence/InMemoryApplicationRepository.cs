using System.Collections.Concurrent;
using WorkPac.Recruitment.Shared.Domain;
using WorkPac.Recruitment.Shared.Enums;
using WorkPac.Recruitment.Shared.Interfaces;

namespace WorkPac.Recruitment.Infrastructure.Persistence;

public class InMemoryApplicationRepository : IApplicationRepository
{
    private readonly ConcurrentDictionary<Guid, Application> _store = new();

    public Task<Application?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        _store.TryGetValue(id, out var application);
        return Task.FromResult(application);
    }

    public Task<List<Application>> GetByJobPostingAsync(Guid jobPostingId, CancellationToken ct = default)
    {
        var results = _store.Values
            .Where(a => a.JobPostingId == jobPostingId)
            .OrderByDescending(a => a.CreatedAt)
            .ToList();
        return Task.FromResult(results);
    }

    public Task<List<Application>> GetByCandidateAsync(Guid candidateId, CancellationToken ct = default)
    {
        var results = _store.Values
            .Where(a => a.CandidateId == candidateId)
            .OrderByDescending(a => a.CreatedAt)
            .ToList();
        return Task.FromResult(results);
    }

    public Task<bool> ExistsAsync(Guid jobPostingId, Guid candidateId, CancellationToken ct = default)
    {
        var exists = _store.Values
            .Any(a => a.JobPostingId == jobPostingId && a.CandidateId == candidateId);
        return Task.FromResult(exists);
    }

    public Task AddAsync(Application application, CancellationToken ct = default)
    {
        _store.TryAdd(application.Id, application);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Application application, CancellationToken ct = default)
    {
        _store.TryUpdate(application.Id, application, _store.GetValueOrDefault(application.Id)!);
        return Task.CompletedTask;
    }

    public Task<List<Application>> GetByStatusAsync(ApplicationStatus status, CancellationToken ct = default)
    {
        var results = _store.Values
            .Where(a => a.Status == status)
            .OrderByDescending(a => a.CreatedAt)
            .ToList();
        return Task.FromResult(results);
    }
}
