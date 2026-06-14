using WorkPac.Recruitment.Contracts.ApiModels;
using WorkPac.Recruitment.Shared.Interfaces;

namespace WorkPac.Recruitment.Applications.Api.Features.ListDocuments;

public class ListDocumentsHandler
{
    private readonly IApplicationRepository _appRepo;
    private readonly ICandidateRepository _candidateRepo;

    public ListDocumentsHandler(
        IApplicationRepository appRepo,
        ICandidateRepository candidateRepo)
    {
        _appRepo = appRepo;
        _candidateRepo = candidateRepo;
    }

    public async Task<PaginatedList<DocumentListItem>> HandleAsync(int page, int pageSize, CancellationToken ct)
    {
        var applications = await _appRepo.GetAllAsync(ct);

        var candidateIds = applications.Select(a => a.CandidateId).Distinct();
        var candidates = await ResolveCandidateNamesAsync(candidateIds, ct);

        var allDocs = applications
            .SelectMany(a => a.Documents.Select(d => new DocumentListItem(
                d.Id, d.FileName, d.ContentType, d.SizeBytes, d.UploadedAt,
                a.Id, a.CandidateId,
                candidates.GetValueOrDefault(a.CandidateId, ""))))
            .OrderByDescending(d => d.UploadedAt)
            .ToList();

        var total = allDocs.Count;
        var items = allDocs
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PaginatedList<DocumentListItem>(items, total, page, pageSize);
    }

    public async Task<PaginatedList<DocumentListItem>> HandleByApplicationAsync(
        Guid applicationId, int page, int pageSize, CancellationToken ct)
    {
        var app = await _appRepo.GetByIdAsync(applicationId, ct);
        if (app is null)
            return new PaginatedList<DocumentListItem>([], 0, page, pageSize);

        var candidate = await _candidateRepo.GetByIdAsync(app.CandidateId, ct);
        var candidateName = candidate is not null ? $"{candidate.FirstName} {candidate.LastName}" : "";

        var allDocs = app.Documents
            .Select(d => new DocumentListItem(
                d.Id, d.FileName, d.ContentType, d.SizeBytes, d.UploadedAt,
                app.Id, app.CandidateId, candidateName))
            .OrderByDescending(d => d.UploadedAt)
            .ToList();

        var total = allDocs.Count;
        var items = allDocs
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PaginatedList<DocumentListItem>(items, total, page, pageSize);
    }

    private async Task<Dictionary<Guid, string>> ResolveCandidateNamesAsync(
        IEnumerable<Guid> candidateIds, CancellationToken ct)
    {
        var result = new Dictionary<Guid, string>();
        foreach (var id in candidateIds)
        {
            var candidate = await _candidateRepo.GetByIdAsync(id, ct);
            if (candidate is not null)
                result[id] = $"{candidate.FirstName} {candidate.LastName}";
        }
        return result;
    }
}
