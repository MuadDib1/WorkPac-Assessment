namespace WorkPac.Recruitment.Matching.Service.Scoring;

public class CertificationsScorer
{
    public double Score(List<string> candidateCerts, List<string> requiredCerts)
    {
        if (requiredCerts.Count == 0) return 1.0;
        if (candidateCerts.Count == 0) return 0.0;

        var candidateSet = new HashSet<string>(candidateCerts, StringComparer.OrdinalIgnoreCase);
        var matched = requiredCerts.Count(c => candidateSet.Contains(c));

        return (double)matched / requiredCerts.Count;
    }
}
