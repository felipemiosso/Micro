using Micro.API.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Micro.API.Data.Configuration;

public class RequisitionOpeningConfiguration : IEntityTypeConfiguration<RequisitionOpening>
{
    public void Configure(EntityTypeBuilder<RequisitionOpening> builder)
    {
        builder.HasKey(o => o.Id);

        builder.HasOne(o => o.Candidate)
            .WithMany()
            .HasForeignKey(o => o.CandidateId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
