using WorkPac.Recruitment.Shared.ValueObjects;

namespace WorkPac.Recruitment.Shared.Domain;

public class Candidate : BaseEntity
{
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string? Phone { get; private set; }
    public Location Location { get; private set; } = new();
    public bool WillingToRelocate { get; private set; }
    public bool FifoWilling { get; private set; }
    public List<string> Skills { get; private set; } = [];
    public int TotalExperienceYears { get; private set; }
    public List<string> Certifications { get; private set; } = [];
    public CandidateStatus Status { get; private set; } = CandidateStatus.Active;

    private Candidate() { }

    public static Candidate Create(
        string firstName, string lastName, string email,
        string? phone, Location location)
    {
        return new Candidate
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Phone = phone,
            Location = location
        };
    }
}

public enum CandidateStatus
{
    Active,
    Placed,
    Unavailable,
    Blacklisted
}
