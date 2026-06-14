using WorkPac.Recruitment.Shared.Domain;

namespace WorkPac.Recruitment.Matching.Service.Scoring;

public class LocationScorer
{
    public double Score(Candidate candidate, JobPosting job)
    {
        if (string.IsNullOrEmpty(job.Location.SiteName) && string.IsNullOrEmpty(job.Location.State))
            return 1.0;

        if (!string.IsNullOrEmpty(job.Location.SiteName))
        {
            if (string.Equals(candidate.Location.SiteName, job.Location.SiteName, StringComparison.OrdinalIgnoreCase))
                return 1.0;
            if (candidate.FifoWilling || candidate.WillingToRelocate)
                return 0.8;
        }

        if (!string.IsNullOrEmpty(job.Location.State) &&
            string.Equals(candidate.Location.State, job.Location.State, StringComparison.OrdinalIgnoreCase))
            return 0.5;

        return 0.0;
    }
}
