using Micro.API.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Micro.API.Data.Configuration;

public class JobPostingConfiguration : IEntityTypeConfiguration<JobPosting>
{
    public void Configure(EntityTypeBuilder<JobPosting> builder)
    {
        builder.HasOne(jp => jp.Requisition)
            .WithOne()
            .HasForeignKey<JobPosting>(jp => jp.RequisitionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
