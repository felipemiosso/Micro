using Micro.API.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Micro.API.Data.Configuration;

public class ApplicationConfiguration : IEntityTypeConfiguration<Application>
{
    public void Configure(EntityTypeBuilder<Application> builder)
    {
        builder.HasOne(a => a.JobPosting)
            .WithMany(jp => jp.Applications)
            .HasForeignKey(a => a.JobPostingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(a => a.CandidateName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.CandidateEmail)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(a => new { a.JobPostingId, a.CandidateEmail })
            .IsUnique();

        builder.HasIndex(a => a.Status);
    }
}
