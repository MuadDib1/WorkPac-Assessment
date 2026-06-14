using WorkPac.Recruitment.Shared.Enums;
using WorkPac.Recruitment.Shared.ValueObjects;

namespace WorkPac.Recruitment.Shared.Domain;

public class JobPosting : BaseEntity
{
    public Guid ClientId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public JobCategory Category { get; private set; }
    public Location Location { get; private set; } = new();
    public PayRate PayRate { get; private set; } = new();
    public EmploymentType EmploymentType { get; private set; }
    public string RosterType { get; private set; } = string.Empty;
    public List<string> RequiredSkills { get; private set; } = [];
    public List<string> RequiredCertifications { get; private set; } = [];
    public List<string> RequiredLicenses { get; private set; } = [];
    public List<string> RequiredQualifications { get; private set; } = [];
    public int RequiredExperienceYears { get; private set; }
    public JobStatus Status { get; private set; } = JobStatus.Draft;
    public DateTime? PublishedAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public Guid CreatedBy { get; private set; }
    public Guid? AssignedTo { get; private set; }

    private JobPosting() { }

    public static JobPosting Create(
        Guid clientId, string title, string description, JobCategory category,
        Location location, PayRate payRate, EmploymentType employmentType,
        int requiredExperienceYears, Guid createdBy)
    {
        return new JobPosting
        {
            ClientId = clientId,
            Title = title,
            Description = description,
            Category = category,
            Location = location,
            PayRate = payRate,
            EmploymentType = employmentType,
            RequiredExperienceYears = requiredExperienceYears,
            Status = JobStatus.Draft,
            CreatedBy = createdBy
        };
    }

    public void Publish(DateTime? expiresAt = null)
    {
        Status = JobStatus.Published;
        PublishedAt = DateTime.UtcNow;
        ExpiresAt = expiresAt ?? DateTime.UtcNow.AddDays(30);
    }

    public void Expire()
    {
        Status = JobStatus.Expired;
    }
}

public enum JobStatus
{
    Draft,
    Published,
    Filled,
    Expired,
    Cancelled
}
