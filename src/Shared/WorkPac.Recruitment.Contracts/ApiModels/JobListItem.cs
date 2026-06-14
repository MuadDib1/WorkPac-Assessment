using WorkPac.Recruitment.Shared.Domain;
using WorkPac.Recruitment.Shared.Enums;

namespace WorkPac.Recruitment.Contracts.ApiModels;

public record JobListItem(
    Guid Id,
    string Title,
    JobCategory Category,
    string Location,
    EmploymentType EmploymentType,
    JobStatus Status
);
