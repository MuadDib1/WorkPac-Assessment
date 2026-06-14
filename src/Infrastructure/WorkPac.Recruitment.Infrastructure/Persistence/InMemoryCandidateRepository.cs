using System.Collections.Concurrent;
using WorkPac.Recruitment.Shared.Domain;
using WorkPac.Recruitment.Shared.Interfaces;

namespace WorkPac.Recruitment.Infrastructure.Persistence;

public class InMemoryCandidateRepository : ICandidateRepository
{
    private readonly ConcurrentDictionary<Guid, Candidate> _store = new();

    public Task<List<Candidate>> GetAllAsync(CancellationToken ct = default)
    {
        return Task.FromResult(_store.Values.ToList());
    }

    public Task<Candidate?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        _store.TryGetValue(id, out var candidate);
        return Task.FromResult(candidate);
    }

    public Task AddAsync(Candidate candidate, CancellationToken ct = default)
    {
        _store.TryAdd(candidate.Id, candidate);
        return Task.CompletedTask;
    }
}
