using WorkPac.Recruitment.Contracts.ApiModels;
using WorkPac.Recruitment.Shared.Interfaces;

namespace WorkPac.Recruitment.Applications.Api.Features.ListApplications;

public class ListApplicationsHandler
{
    private readonly IApplicationRepository _applicationRepo;
    private readonly IJobPostingRepository _jobRepo;
    private readonly ICandidateRepository _candidateRepo;

    public ListApplicationsHandler(
        IApplicationRepository applicationRepo,
        IJobPostingRepository jobRepo,
        ICandidateRepository candidateRepo)
    {
        _applicationRepo = applicationRepo;
        _jobRepo = jobRepo;
        _candidateRepo = candidateRepo;
    }

    public async Task<PaginatedList<ApplicationListItem>> HandleByJobAsync(
        Guid jobPostingId, int page, int pageSize, CancellationToken ct)
    {
        var applications = await _applicationRepo.GetByJobPostingAsync(jobPostingId, ct);
        var job = await _jobRepo.GetByIdAsync(jobPostingId, ct);
        var jobTitle = job?.Title ?? "";
        var total = applications.Count;

        var paged = applications
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var candidates = await ResolveCandidatesAsync(paged.Select(a => a.CandidateId), ct);

        var items = paged.Select(a => new ApplicationListItem(
            a.Id, a.JobPostingId, jobTitle, a.CandidateId,
            candidates.GetValueOrDefault(a.CandidateId, ""),
            a.Status, a.MatchScore, a.CreatedAt)).ToList();

        return new PaginatedList<ApplicationListItem>(items, total, page, pageSize);
    }

    public async Task<PaginatedList<ApplicationListItem>> HandleByCandidateAsync(
        Guid candidateId, int page, int pageSize, CancellationToken ct)
    {
        var applications = await _applicationRepo.GetByCandidateAsync(candidateId, ct);
        var candidate = await _candidateRepo.GetByIdAsync(candidateId, ct);
        var candidateName = candidate != null ? $"{candidate.FirstName} {candidate.LastName}" : "";
        var total = applications.Count;

        var paged = applications
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var jobs = await ResolveJobsAsync(paged.Select(a => a.JobPostingId), ct);

        var items = paged.Select(a => new ApplicationListItem(
            a.Id, a.JobPostingId, jobs.GetValueOrDefault(a.JobPostingId, ""),
            a.CandidateId, candidateName,
            a.Status, a.MatchScore, a.CreatedAt)).ToList();

        return new PaginatedList<ApplicationListItem>(items, total, page, pageSize);
    }

    private async Task<Dictionary<Guid, string>> ResolveCandidatesAsync(IEnumerable<Guid> candidateIds, CancellationToken ct)
    {
        var result = new Dictionary<Guid, string>();
        foreach (var id in candidateIds.Distinct())
        {
            var candidate = await _candidateRepo.GetByIdAsync(id, ct);
            if (candidate is not null)
                result[id] = $"{candidate.FirstName} {candidate.LastName}";
        }
        return result;
    }

    private async Task<Dictionary<Guid, string>> ResolveJobsAsync(IEnumerable<Guid> jobIds, CancellationToken ct)
    {
        var result = new Dictionary<Guid, string>();
        foreach (var id in jobIds.Distinct())
        {
            var job = await _jobRepo.GetByIdAsync(id, ct);
            if (job is not null)
                result[id] = job.Title;
        }
        return result;
    }
}
