using WorkPac.Recruitment.Contracts.ApiModels;
using WorkPac.Recruitment.Contracts.Messaging;
using WorkPac.Recruitment.Shared.Exceptions;
using WorkPac.Recruitment.Shared.Interfaces;

namespace WorkPac.Recruitment.Applications.Api.Features.UpdateApplicationStatus;

public class UpdateApplicationStatusHandler
{
    private readonly IApplicationRepository _applicationRepo;
    private readonly IEventBus _eventBus;

    public UpdateApplicationStatusHandler(IApplicationRepository applicationRepo, IEventBus eventBus)
    {
        _applicationRepo = applicationRepo;
        _eventBus = eventBus;
    }

    public async Task<ApplicationResponse> HandleAsync(Guid id, UpdateApplicationStatusRequest request, CancellationToken ct)
    {
        var application = await _applicationRepo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Application", id);

        var oldStatus = application.Status;
        application.UpdateStatus(request.NewStatus, request.ChangedByUserId, request.Reason);
        await _applicationRepo.UpdateAsync(application, ct);

        await _eventBus.PublishAsync(new ApplicationStatusChangedEvent(
            id, oldStatus, request.NewStatus, request.Reason, request.ChangedByUserId), ct);

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
