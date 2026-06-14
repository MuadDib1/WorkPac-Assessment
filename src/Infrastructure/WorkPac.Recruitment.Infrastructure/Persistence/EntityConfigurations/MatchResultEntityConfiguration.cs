using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkPac.Recruitment.Shared.Domain;

namespace WorkPac.Recruitment.Infrastructure.Persistence.EntityConfigurations;

public class MatchResultEntityConfiguration : IEntityTypeConfiguration<MatchResult>
{
    public void Configure(EntityTypeBuilder<MatchResult> builder)
    {
        builder.ToTable("MatchResults");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Score).HasPrecision(5, 4).IsRequired();
        builder.Property(x => x.CandidateId).IsRequired();
        builder.Property(x => x.JobPostingId).IsRequired();

        builder.OwnsOne(x => x.Breakdown, b =>
        {
            b.Property(d => d.SkillsScore).HasPrecision(5, 4);
            b.Property(d => d.ExperienceScore).HasPrecision(5, 4);
            b.Property(d => d.LocationScore).HasPrecision(5, 4);
            b.Property(d => d.CertificationsScore).HasPrecision(5, 4);
            b.Property(d => d.AvailabilityScore).HasPrecision(5, 4);
        });

        builder.HasIndex(x => new { x.CandidateId, x.JobPostingId });
    }
}
