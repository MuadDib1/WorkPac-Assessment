using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkPac.Recruitment.Shared.Domain;

namespace WorkPac.Recruitment.Infrastructure.Persistence.EntityConfigurations;

public class JobPostingEntityConfiguration : IEntityTypeConfiguration<JobPosting>
{
    public void Configure(EntityTypeBuilder<JobPosting> builder)
    {
        builder.ToTable("JobPostings");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Description).HasMaxLength(10000);
        builder.Property(x => x.Category).HasConversion<string>().HasMaxLength(50);
        builder.Property(x => x.EmploymentType).HasConversion<string>().HasMaxLength(50);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(50);
        builder.Property(x => x.ClientId).IsRequired();
        builder.Property(x => x.RowVersion).IsRowVersion();

        builder.OwnsOne(x => x.Location, locBuilder =>
        {
            locBuilder.Property(l => l.Suburb).HasMaxLength(100);
            locBuilder.Property(l => l.State).HasMaxLength(50);
            locBuilder.Property(l => l.SiteName).HasMaxLength(200);
        });

        builder.OwnsOne(x => x.PayRate, payBuilder =>
        {
            payBuilder.Property(p => p.Amount).HasPrecision(10, 2);
            payBuilder.Property(p => p.Currency).HasMaxLength(3);
            payBuilder.Property(p => p.Interval).HasConversion<string>().HasMaxLength(20);
        });

        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.ClientId);
    }
}
