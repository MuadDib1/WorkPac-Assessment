using WorkPac.Recruitment.Contracts.ApiModels;
using WorkPac.Recruitment.Shared.Interfaces;

namespace WorkPac.Recruitment.Applications.Api.Features.ListCandidates;

public class ListCandidatesHandler
{
    private readonly ICandidateRepository _repo;

    public ListCandidatesHandler(ICandidateRepository repo)
    {
        _repo = repo;
    }

    public async Task<PaginatedList<CandidateListItem>> HandleAsync(int page, int pageSize, CancellationToken ct)
    {
        var candidates = await _repo.GetAllAsync(ct);
        var total = candidates.Count;
        var items = candidates
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CandidateListItem(
                c.Id, c.FirstName, c.LastName, c.Email,
                c.Location.DisplayName, c.Skills.Count,
                c.TotalExperienceYears, c.Status))
            .ToList();
        return new PaginatedList<CandidateListItem>(items, total, page, pageSize);
    }
}
