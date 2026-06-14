using Microsoft.EntityFrameworkCore;
using WorkPac.Recruitment.Infrastructure.Persistence.EntityConfigurations;
using WorkPac.Recruitment.Shared;
using WorkPac.Recruitment.Shared.Domain;

namespace WorkPac.Recruitment.Infrastructure.Persistence;

public class RecruitmentDbContext : DbContext
{
    public DbSet<Application> Applications => Set<Application>();
    public DbSet<JobPosting> JobPostings => Set<JobPosting>();
    public DbSet<Candidate> Candidates => Set<Candidate>();
    public DbSet<MatchResult> MatchResults => Set<MatchResult>();

    public RecruitmentDbContext(DbContextOptions<RecruitmentDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ApplicationEntityConfiguration());
        modelBuilder.ApplyConfiguration(new JobPostingEntityConfiguration());
        modelBuilder.ApplyConfiguration(new CandidateEntityConfiguration());
        modelBuilder.ApplyConfiguration(new MatchResultEntityConfiguration());
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();
        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Modified)
                entry.Entity.GetType().GetProperty("UpdatedAt")?.SetValue(entry.Entity, DateTime.UtcNow);
        }
    }
}
