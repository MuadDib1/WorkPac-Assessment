using WorkPac.Recruitment.Shared.Domain;

namespace WorkPac.Recruitment.Contracts.ApiModels;

public record CandidateListItem(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string Location,
    int SkillCount,
    int TotalExperienceYears,
    CandidateStatus Status
);
