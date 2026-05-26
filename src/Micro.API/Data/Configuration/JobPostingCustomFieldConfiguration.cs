using Micro.API.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Micro.API.Data.Configuration;

public class JobPostingCustomFieldConfiguration : IEntityTypeConfiguration<JobPostingCustomField>
{
    public void Configure(EntityTypeBuilder<JobPostingCustomField> builder)
    {
        builder.ToTable("JobPostingCustomFields");
        builder.HasKey(x => new { x.JobPostingId, x.CustomFieldDefinitionId });

        builder.HasOne(x => x.JobPosting)
            .WithMany()
            .HasForeignKey(x => x.JobPostingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.CustomFieldDefinition)
            .WithMany()
            .HasForeignKey(x => x.CustomFieldDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
