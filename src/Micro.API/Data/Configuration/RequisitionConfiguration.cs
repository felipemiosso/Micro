using Micro.API.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Micro.API.Data.Configuration;

public class RequisitionConfiguration : IEntityTypeConfiguration<Requisition>
{
    public void Configure(EntityTypeBuilder<Requisition> builder)
    {
        builder.HasKey(r => r.Id);
        
        builder.HasOne(r => r.Department)
            .WithMany()
            .HasForeignKey(r => r.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.SalaryBand)
            .WithMany()
            .HasForeignKey(r => r.SalaryBandId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.CostCenter)
            .WithMany()
            .HasForeignKey(r => r.CostCenterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(r => r.Openings)
            .WithOne(o => o.Requisition)
            .HasForeignKey(o => o.RequisitionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
