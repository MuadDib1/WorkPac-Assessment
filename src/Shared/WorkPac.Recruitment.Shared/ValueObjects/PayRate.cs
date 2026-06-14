namespace WorkPac.Recruitment.Shared.ValueObjects;

public record PayRate
{
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "AUD";
    public RateInterval Interval { get; init; } = RateInterval.Hourly;

    public override string ToString() => $"{Currency} {Amount:F2}/{Interval.ToString().ToLower()}";
}

public enum RateInterval
{
    Hourly,
    Daily,
    Weekly,
    Annually
}
