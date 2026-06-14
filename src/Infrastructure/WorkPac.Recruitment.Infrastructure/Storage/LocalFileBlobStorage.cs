using Microsoft.Extensions.Logging;
using WorkPac.Recruitment.Shared.Interfaces;

namespace WorkPac.Recruitment.Infrastructure.Storage;

public class LocalFileBlobStorage : IBlobStorage
{
    private readonly string _basePath;
    private readonly ILogger<LocalFileBlobStorage> _logger;

    public LocalFileBlobStorage(string basePath, ILogger<LocalFileBlobStorage> logger)
    {
        _basePath = Path.Combine(basePath, "blobstorage");
        _logger = logger;
        Directory.CreateDirectory(_basePath);
    }

    public async Task<string> UploadAsync(string container, string blobName, Stream content, string contentType, CancellationToken ct = default)
    {
        var dir = Path.Combine(_basePath, container);
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, blobName);

        await using var fileStream = File.Create(path);
        await content.CopyToAsync(fileStream, ct);

        _logger.LogDebug("Uploaded blob {Container}/{BlobName}", container, blobName);
        return path;
    }

    public Task<Stream?> DownloadAsync(string container, string blobName, CancellationToken ct = default)
    {
        var path = Path.Combine(_basePath, container, blobName);
        if (!File.Exists(path))
            return Task.FromResult<Stream?>(null);

        return Task.FromResult<Stream?>(File.OpenRead(path));
    }

    public Task DeleteAsync(string container, string blobName, CancellationToken ct = default)
    {
        var path = Path.Combine(_basePath, container, blobName);
        if (File.Exists(path))
            File.Delete(path);
        return Task.CompletedTask;
    }
}
