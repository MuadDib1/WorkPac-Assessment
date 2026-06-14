namespace WorkPac.Recruitment.Shared.Interfaces;

public interface IBlobStorage
{
    Task<string> UploadAsync(string container, string blobName, Stream content, string contentType, CancellationToken ct = default);
    Task<Stream?> DownloadAsync(string container, string blobName, CancellationToken ct = default);
    Task DeleteAsync(string container, string blobName, CancellationToken ct = default);
}
