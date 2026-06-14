using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkPac.Recruitment.Shared.Domain;

namespace WorkPac.Recruitment.Infrastructure.Persistence.EntityConfigurations;

public class CandidateEntityConfiguration : IEntityTypeConfiguration<Candidate>
{
    public void Configure(EntityTypeBuilder<Candidate> builder)
    {
        builder.ToTable("Candidates");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.LastName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Email).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Phone).HasMaxLength(30);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(50);
        builder.Property(x => x.RowVersion).IsRowVersion();

        builder.OwnsOne(x => x.Location, locBuilder =>
        {
            locBuilder.Property(l => l.Suburb).HasMaxLength(100);
            locBuilder.Property(l => l.State).HasMaxLength(50);
        });

        builder.HasIndex(x => x.Email).IsUnique();
        builder.HasIndex(x => x.Status);
    }
}
