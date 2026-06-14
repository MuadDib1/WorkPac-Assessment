using WorkPac.Recruitment.Contracts.ApiModels;
using WorkPac.Recruitment.Shared.Exceptions;
using WorkPac.Recruitment.Shared.Interfaces;
using WorkPac.Recruitment.Shared.ValueObjects;

namespace WorkPac.Recruitment.Applications.Api.Features.UploadDocument;

public class UploadDocumentHandler
{
    private readonly IApplicationRepository _applicationRepo;
    private readonly IBlobStorage _blobStorage;

    public UploadDocumentHandler(IApplicationRepository applicationRepo, IBlobStorage blobStorage)
    {
        _applicationRepo = applicationRepo;
        _blobStorage = blobStorage;
    }

    public async Task<DocumentInfo> HandleAsync(Guid applicationId, string fileName, string contentType, Stream content, CancellationToken ct)
    {
        var application = await _applicationRepo.GetByIdAsync(applicationId, ct)
            ?? throw new NotFoundException("Application", applicationId);

        var blobPath = await _blobStorage.UploadAsync("applications", $"{applicationId}/{fileName}", content, contentType, ct);

        var docRef = new DocumentReference
        {
            FileName = fileName,
            ContentType = contentType,
            BlobPath = blobPath
        };

        application.AddDocument(docRef);
        await _applicationRepo.UpdateAsync(application, ct);

        return new DocumentInfo(docRef.Id, docRef.FileName, docRef.ContentType, docRef.SizeBytes, docRef.UploadedAt);
    }
}
