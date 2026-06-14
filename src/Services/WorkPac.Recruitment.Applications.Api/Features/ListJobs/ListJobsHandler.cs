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

    public async Task<List<JobListItem>> HandleAsync(CancellationToken ct)
    {
        var jobs = await _repo.GetAllAsync(ct);
        return jobs.Select(j => new JobListItem(
            j.Id, j.Title, j.Category, j.Location.DisplayName,
            j.EmploymentType, j.Status)).ToList();
    }
}
