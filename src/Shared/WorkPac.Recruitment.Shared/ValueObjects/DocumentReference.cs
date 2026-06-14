namespace WorkPac.Recruitment.Shared.ValueObjects;

public record DocumentReference
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long SizeBytes { get; init; }
    public string BlobPath { get; init; } = string.Empty;
    public DateTime UploadedAt { get; init; } = DateTime.UtcNow;
}
