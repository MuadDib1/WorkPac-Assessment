using WorkPac.Recruitment.Shared.Domain;

namespace WorkPac.Recruitment.Matching.Service.Scoring;

public class AvailabilityScorer
{
    public double Score(Candidate candidate)
    {
        return candidate.Status switch
        {
            CandidateStatus.Active => 1.0,
            CandidateStatus.Placed => 0.0,
            CandidateStatus.Unavailable => 0.0,
            _ => 0.0
        };
    }
}
