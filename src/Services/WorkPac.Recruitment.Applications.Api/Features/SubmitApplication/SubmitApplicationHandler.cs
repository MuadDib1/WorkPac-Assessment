using WorkPac.Recruitment.Contracts.ApiModels;
using WorkPac.Recruitment.Contracts.Messaging;
using WorkPac.Recruitment.Shared.Domain;
using WorkPac.Recruitment.Shared.Exceptions;
using WorkPac.Recruitment.Shared.Interfaces;
using WorkPac.Recruitment.Shared.ValueObjects;

namespace WorkPac.Recruitment.Applications.Api.Features.SubmitApplication;

public class SubmitApplicationHandler
{
    private readonly IApplicationRepository _applicationRepo;
    private readonly IJobPostingRepository _jobPostingRepo;
    private readonly ICandidateRepository _candidateRepo;
    private readonly IEventBus _eventBus;

    public SubmitApplicationHandler(
        IApplicationRepository applicationRepo,
        IJobPostingRepository jobPostingRepo,
        ICandidateRepository candidateRepo,
        IEventBus eventBus)
    {
        _applicationRepo = applicationRepo;
        _jobPostingRepo = jobPostingRepo;
        _candidateRepo = candidateRepo;
        _eventBus = eventBus;
    }

    public async Task<ApplicationResponse> HandleAsync(Guid jobId, SubmitApplicationRequest request, CancellationToken ct)
    {
        var jobPosting = await _jobPostingRepo.GetByIdAsync(jobId, ct)
            ?? throw new NotFoundException("JobPosting", jobId);

        var candidate = await _candidateRepo.GetByIdAsync(request.CandidateId, ct)
            ?? throw new NotFoundException("Candidate", request.CandidateId);

        var alreadyExists = await _applicationRepo.ExistsAsync(jobId, request.CandidateId, ct);
        if (alreadyExists)
            throw new ConflictException($"Candidate {request.CandidateId} has already applied to job {jobId}.");

        var application = Shared.Domain.Application.Submit(jobId, request.CandidateId, request.CoverLetter);

        if (request.Documents?.Count > 0)
        {
            foreach (var doc in request.Documents)
            {
                application.AddDocument(new DocumentReference
                {
                    FileName = doc.FileName,
                    ContentType = doc.ContentType,
                    SizeBytes = Convert.FromBase64String(doc.Base64Content ?? "").Length,
                    BlobPath = $"applications/{application.Id}/{doc.FileName}"
                });
            }
        }

        await _applicationRepo.AddAsync(application, ct);

        await _eventBus.PublishAsync(new ApplicationSubmittedEvent(
            application.Id, jobId, request.CandidateId, application.CreatedAt), ct);

        return MapToResponse(application, jobPosting.Title, $"{candidate.FirstName} {candidate.LastName}");
    }

    private static ApplicationResponse MapToResponse(Shared.Domain.Application app, string jobTitle, string candidateName)
    {
        return new ApplicationResponse(
            app.Id,
            app.JobPostingId,
            app.CandidateId,
            app.Status,
            app.CoverLetter,
            app.Documents.Select(d => new DocumentInfo(d.Id, d.FileName, d.ContentType, d.SizeBytes, d.UploadedAt)).ToList(),
            app.StatusHistory.Select(h => new Contracts.ApiModels.StatusHistoryEntry(h.FromStatus, h.ToStatus, h.ChangedBy, h.Reason, h.ChangedAt)).ToList(),
            app.MatchScore,
            app.CreatedAt,
            app.UpdatedAt);
    }
}
