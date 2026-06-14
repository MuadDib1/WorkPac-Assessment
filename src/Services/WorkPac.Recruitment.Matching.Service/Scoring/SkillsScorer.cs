namespace WorkPac.Recruitment.Matching.Service.Scoring;

public class SkillsScorer
{
    public double Score(List<string> candidateSkills, List<string> requiredSkills)
    {
        if (requiredSkills.Count == 0) return 1.0;
        if (candidateSkills.Count == 0) return 0.0;

        var candidateSet = new HashSet<string>(candidateSkills, StringComparer.OrdinalIgnoreCase);
        var requiredSet = new HashSet<string>(requiredSkills, StringComparer.OrdinalIgnoreCase);

        var intersection = candidateSet.Intersect(requiredSet, StringComparer.OrdinalIgnoreCase).Count();
        var union = candidateSet.Union(requiredSet, StringComparer.OrdinalIgnoreCase).Count();

        return union == 0 ? 0.0 : (double)intersection / union;
    }
}
