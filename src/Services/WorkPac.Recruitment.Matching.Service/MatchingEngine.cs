using WorkPac.Recruitment.Matching.Service.Scoring;
using WorkPac.Recruitment.Shared.Domain;

namespace WorkPac.Recruitment.Matching.Service;

public class MatchingEngine
{
    private readonly SkillsScorer _skillsScorer;
    private readonly ExperienceScorer _experienceScorer;
    private readonly LocationScorer _locationScorer;
    private readonly CertificationsScorer _certificationsScorer;
    private readonly AvailabilityScorer _availabilityScorer;

    public MatchingEngine(
        SkillsScorer skillsScorer,
        ExperienceScorer experienceScorer,
        LocationScorer locationScorer,
        CertificationsScorer certificationsScorer,
        AvailabilityScorer availabilityScorer)
    {
        _skillsScorer = skillsScorer;
        _experienceScorer = experienceScorer;
        _locationScorer = locationScorer;
        _certificationsScorer = certificationsScorer;
        _availabilityScorer = availabilityScorer;
    }

    public MatchResult CalculateMatch(Candidate candidate, JobPosting job)
    {
        var skillsScore = _skillsScorer.Score(candidate.Skills, job.RequiredSkills);
        var experienceScore = _experienceScorer.Score(candidate.TotalExperienceYears, job.RequiredExperienceYears);
        var locationScore = _locationScorer.Score(candidate, job);
        var certScore = _certificationsScorer.Score(candidate.Certifications, job.RequiredCertifications);
        var availabilityScore = _availabilityScorer.Score(candidate);

        var totalScore = 0.30 * skillsScore
                       + 0.25 * experienceScore
                       + 0.15 * locationScore
                       + 0.20 * certScore
                       + 0.10 * availabilityScore;

        return new MatchResult(candidate.Id, job.Id, totalScore, new MatchBreakdown
        {
            SkillsScore = skillsScore,
            ExperienceScore = experienceScore,
            LocationScore = locationScore,
            CertificationsScore = certScore,
            AvailabilityScore = availabilityScore
        });
    }
}
