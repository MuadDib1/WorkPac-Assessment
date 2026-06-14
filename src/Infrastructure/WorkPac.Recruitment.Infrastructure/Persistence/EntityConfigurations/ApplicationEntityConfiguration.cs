using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkPac.Recruitment.Infrastructure.Persistence.ValueConverters;
using WorkPac.Recruitment.Shared.Domain;

namespace WorkPac.Recruitment.Infrastructure.Persistence.EntityConfigurations;

public class ApplicationEntityConfiguration : IEntityTypeConfiguration<Application>
{
    public void Configure(EntityTypeBuilder<Application> builder)
    {
        builder.ToTable("Applications");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.CandidateId).IsRequired();
        builder.Property(x => x.JobPostingId).IsRequired();
        builder.Property(x => x.Status).IsRequired().HasConversion<string>().HasMaxLength(50);
        builder.Property(x => x.CoverLetter).HasMaxLength(5000);
        builder.Property(x => x.MatchScore).HasPrecision(5, 4);
        builder.Property(x => x.RowVersion).IsRowVersion();

        builder.OwnsOne(x => x.Documents, docBuilder =>
        {
            docBuilder.Property<Guid>("Id");
            docBuilder.WithOwner();
        });

        builder.OwnsMany(x => x.StatusHistory, historyBuilder =>
        {
            historyBuilder.ToTable("ApplicationStatusHistory");
            historyBuilder.WithOwner().HasForeignKey("ApplicationId");
            historyBuilder.Property(x => x.FromStatus).HasConversion<string>().HasMaxLength(50);
            historyBuilder.Property(x => x.ToStatus).HasConversion<string>().HasMaxLength(50);
            historyBuilder.Property(x => x.ChangedBy).HasMaxLength(200);
            historyBuilder.Property(x => x.Reason).HasMaxLength(1000);
        });

        builder.HasIndex(x => new { x.JobPostingId, x.CandidateId }).IsUnique();
        builder.HasIndex(x => x.CandidateId);
        builder.HasIndex(x => x.Status);
    }
}
