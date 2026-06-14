using Microsoft.Extensions.Logging;
using WorkPac.Recruitment.Shared;
using WorkPac.Recruitment.Shared.Domain;
using WorkPac.Recruitment.Shared.Enums;
using WorkPac.Recruitment.Shared.Interfaces;
using WorkPac.Recruitment.Shared.ValueObjects;

namespace WorkPac.Recruitment.Infrastructure.Data;

public class DataSeeder
{
    private readonly ICandidateRepository _candidates;
    private readonly IJobPostingRepository _jobs;
    private readonly IApplicationRepository _applications;
    private readonly ILogger<DataSeeder> _logger;

    public DataSeeder(
        ICandidateRepository candidates,
        IJobPostingRepository jobs,
        IApplicationRepository applications,
        ILogger<DataSeeder> logger)
    {
        _candidates = candidates;
        _jobs = jobs;
        _applications = applications;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        var candidate1 = Candidate.Create("Jack", "Harris", "jack.harris@example.com", "0401 123 456",
            new Location { Suburb = "Mackay", State = "QLD" });
        SetProperty(candidate1, "Skills", new List<string> { "diesel fitting", "hydraulics", "welding", "diagnostics", "preventative maintenance" });
        SetProperty(candidate1, "Certifications", new List<string> { "Standard 11", "Coal Board Medical", "RIISS", "Working at Heights", "Confined Space" });
        SetProperty(candidate1, "TotalExperienceYears", 8);
        SetProperty(candidate1, "FifoWilling", true);
        SetProperty(candidate1, "Status", CandidateStatus.Active);
        await _candidates.AddAsync(candidate1);

        var candidate2 = Candidate.Create("Sarah", "Chen", "sarah.chen@example.com", "0402 234 567",
            new Location { Suburb = "Rockhampton", State = "QLD" });
        SetProperty(candidate2, "Skills", new List<string> { "high voltage", "electrical installations", "PLC programming", "SCADA", "motor control" });
        SetProperty(candidate2, "Certifications", new List<string> { "Standard 11", "Coal Board Medical", "RIISS", "Electrical License", "HV Switching Ticket" });
        SetProperty(candidate2, "TotalExperienceYears", 6);
        SetProperty(candidate2, "FifoWilling", true);
        SetProperty(candidate2, "Status", CandidateStatus.Active);
        await _candidates.AddAsync(candidate2);

        var candidate3 = Candidate.Create("Mick", "O'Brien", "mick.obrien@example.com", "0403 345 678",
            new Location { Suburb = "Toowoomba", State = "QLD" });
        SetProperty(candidate3, "Skills", new List<string> { "heavy diesel", "engine rebuilds", "transmissions", "hydraulics", "air conditioning" });
        SetProperty(candidate3, "Certifications", new List<string> { "Standard 11", "Coal Board Medical", "RIISS", "Forklift License", "Dogging Ticket" });
        SetProperty(candidate3, "TotalExperienceYears", 12);
        SetProperty(candidate3, "FifoWilling", true);
        SetProperty(candidate3, "Status", CandidateStatus.Active);
        await _candidates.AddAsync(candidate3);

        var candidate4 = Candidate.Create("Emma", "Walsh", "emma.walsh@example.com", "0404 456 789",
            new Location { Suburb = "Brisbane", State = "QLD" });
        SetProperty(candidate4, "Skills", new List<string> { "mechanical engineering", "project management", "CAD", "asset management", "root cause analysis" });
        SetProperty(candidate4, "Certifications", new List<string> { "Standard 11", "Coal Board Medical", "RIISS", "Project Management Professional" });
        SetProperty(candidate4, "TotalExperienceYears", 4);
        SetProperty(candidate4, "FifoWilling", false);
        SetProperty(candidate4, "WillingToRelocate", true);
        SetProperty(candidate4, "Status", CandidateStatus.Active);
        await _candidates.AddAsync(candidate4);

        var candidate5 = Candidate.Create("Jake", "Thompson", "jake.thompson@example.com", "0405 567 890",
            new Location { Suburb = "Kalgoorlie", State = "WA" });
        SetProperty(candidate5, "Skills", new List<string> { "drilling", "blasting", "blast design", "explosives handling", "pit operations" });
        SetProperty(candidate5, "Certifications", new List<string> { "Standard 11", "Shotfirer License", "Coal Board Medical", "RIISS", "Dangerous Goods" });
        SetProperty(candidate5, "TotalExperienceYears", 7);
        SetProperty(candidate5, "FifoWilling", true);
        SetProperty(candidate5, "Status", CandidateStatus.Active);
        await _candidates.AddAsync(candidate5);

        var job1 = JobPosting.Create(
            Guid.NewGuid(), "Diesel Fitter", "Heavy diesel fitting for mobile plant at Peak Downs mine site",
            JobCategory.Mining,
            new Location { SiteName = "Peak Downs", State = "QLD", IsFifo = true },
            new PayRate { Amount = 65, Currency = "AUD", Interval = RateInterval.Hourly },
            EmploymentType.Contract, 5, candidate1.Id);
        SetProperty(job1, "RequiredSkills", new List<string> { "diesel fitting", "hydraulics", "welding", "diagnostics" });
        SetProperty(job1, "RequiredCertifications", new List<string> { "Standard 11", "Coal Board Medical", "RIISS" });
        SetProperty(job1, "RosterType", "7/7 DIDO");
        job1.Publish();
        await _jobs.AddAsync(job1);

        var job2 = JobPosting.Create(
            Guid.NewGuid(), "HV Electrician", "High voltage electrician for coal handling plant",
            JobCategory.Mining,
            new Location { SiteName = "Goonyella Riverside", State = "QLD", IsFifo = true },
            new PayRate { Amount = 72, Currency = "AUD", Interval = RateInterval.Hourly },
            EmploymentType.Contract, 3, candidate2.Id);
        SetProperty(job2, "RequiredSkills", new List<string> { "high voltage", "electrical installations", "motor control", "SCADA" });
        SetProperty(job2, "RequiredCertifications", new List<string> { "Standard 11", "Coal Board Medical", "Electrical License", "HV Switching Ticket" });
        SetProperty(job2, "RosterType", "7/7 FIFO");
        job2.Publish();
        await _jobs.AddAsync(job2);

        var job3 = JobPosting.Create(
            Guid.NewGuid(), "Drill & Blast Operator", "Experienced shotfirer for open pit operations",
            JobCategory.Mining,
            new Location { SiteName = "Newman", State = "WA", IsFifo = true },
            new PayRate { Amount = 180000, Currency = "AUD", Interval = RateInterval.Annually },
            EmploymentType.Permanent, 3, candidate5.Id);
        SetProperty(job3, "RequiredSkills", new List<string> { "drilling", "blasting", "blast design", "explosives handling" });
        SetProperty(job3, "RequiredCertifications", new List<string> { "Standard 11", "Shotfirer License", "Dangerous Goods" });
        SetProperty(job3, "RosterType", "8/6 FIFO");
        job3.Publish();
        await _jobs.AddAsync(job3);

        var app1 = Application.Submit(job1.Id, candidate1.Id, "I have 8 years experience as a diesel fitter in QLD coal mines.");
        app1.UpdateStatus(ApplicationStatus.UnderReview, candidate1.Id, null);
        app1.UpdateStatus(ApplicationStatus.Shortlisted, candidate1.Id, "Strong match on skills and experience");
        await _applications.AddAsync(app1);

        var app2 = Application.Submit(job2.Id, candidate2.Id, "HV electrician with 6 years experience in coal handling plants.");
        await _applications.AddAsync(app2);

        var app3 = Application.Submit(job3.Id, candidate5.Id, "Experienced shotfirer with open pit experience in WA.");
        await _applications.AddAsync(app3);

        _logger.LogInformation("Seed data created: {Candidates} candidates, {Jobs} jobs, {Applications} applications",
            5, 3, 3);
        _logger.LogInformation("Candidate IDs: {Ids}", string.Join(", ", GetIds(candidate1, candidate2, candidate3, candidate4, candidate5)));
        _logger.LogInformation("Job IDs: {Ids}", string.Join(", ", GetIds(job1, job2, job3)));
        _logger.LogInformation("Application IDs: {Ids}", string.Join(", ", GetIds(app1, app2, app3)));
    }

    private static void SetProperty<T>(object target, string propertyName, T value)
    {
        target.GetType().GetProperty(propertyName)?.SetValue(target, value);
    }

    private static string GetIds(params BaseEntity[] entities)
    {
        return string.Join(", ", entities.Select(e => $"{e.GetType().Name}: {e.Id}"));
    }
}
