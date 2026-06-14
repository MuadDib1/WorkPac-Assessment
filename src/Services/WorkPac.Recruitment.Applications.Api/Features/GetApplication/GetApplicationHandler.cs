using WorkPac.Recruitment.Contracts.ApiModels;
using WorkPac.Recruitment.Shared.Exceptions;
using WorkPac.Recruitment.Shared.Interfaces;

namespace WorkPac.Recruitment.Applications.Api.Features.GetApplication;

public class GetApplicationHandler
{
    private readonly IApplicationRepository _applicationRepo;

    public GetApplicationHandler(IApplicationRepository applicationRepo)
    {
        _applicationRepo = applicationRepo;
    }

    public async Task<ApplicationResponse> HandleAsync(Guid id, CancellationToken ct)
    {
        var application = await _applicationRepo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Application", id);

        return new ApplicationResponse(
            application.Id,
            application.JobPostingId,
            application.CandidateId,
            application.Status,
            application.CoverLetter,
            application.Documents.Select(d => new DocumentInfo(d.Id, d.FileName, d.ContentType, d.SizeBytes, d.UploadedAt)).ToList(),
            application.StatusHistory.Select(h => new Contracts.ApiModels.StatusHistoryEntry(h.FromStatus, h.ToStatus, h.ChangedBy, h.Reason, h.ChangedAt)).ToList(),
            application.MatchScore,
            application.CreatedAt,
            application.UpdatedAt);
    }
}
