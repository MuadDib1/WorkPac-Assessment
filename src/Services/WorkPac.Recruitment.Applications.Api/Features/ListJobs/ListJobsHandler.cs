using WorkPac.Recruitment.Contracts.ApiModels;
using WorkPac.Recruitment.Shared.Interfaces;

namespace WorkPac.Recruitment.Applications.Api.Features.ListJobs;

public class ListJobsHandler
{
    private readonly IJobPostingRepository _repo;

    public ListJobsHandler(IJobPostingRepository repo)
    {
        _repo = repo;
    }

    public async Task<PaginatedList<JobListItem>> HandleAsync(int page, int pageSize, CancellationToken ct)
    {
        var jobs = await _repo.GetAllAsync(ct);
        var total = jobs.Count;
        var items = jobs
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(j => new JobListItem(
                j.Id, j.Title, j.Category, j.Location.DisplayName,
                j.EmploymentType, j.Status))
            .ToList();
        return new PaginatedList<JobListItem>(items, total, page, pageSize);
    }
}
