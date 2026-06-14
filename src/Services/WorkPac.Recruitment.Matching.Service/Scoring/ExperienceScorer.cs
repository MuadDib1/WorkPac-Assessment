namespace WorkPac.Recruitment.Matching.Service.Scoring;

public class ExperienceScorer
{
    public double Score(int candidateYears, int requiredYears)
    {
        if (requiredYears <= 0) return 1.0;
        if (candidateYears <= 0) return 0.0;

        return Math.Min((double)candidateYears / requiredYears, 1.0);
    }
}
