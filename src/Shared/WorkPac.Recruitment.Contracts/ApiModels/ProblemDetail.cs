namespace WorkPac.Recruitment.Contracts.ApiModels;

public record ProblemDetail
{
    public string Type { get; init; } = "about:blank";
    public string Title { get; init; } = string.Empty;
    public int Status { get; init; }
    public string Detail { get; init; } = string.Empty;
    public string Instance { get; init; } = string.Empty;
    public Dictionary<string, string[]>? Errors { get; init; }
    public string? CorrelationId { get; init; }
}
