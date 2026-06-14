namespace WorkPac.Recruitment.Shared.ValueObjects;

public record Location
{
    public string Suburb { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public string? SiteName { get; init; }
    public bool IsFifo { get; init; }
    public bool IsDido { get; init; }

    public string DisplayName =>
        SiteName ?? (string.IsNullOrEmpty(Suburb) ? State : $"{Suburb}, {State}");
}
