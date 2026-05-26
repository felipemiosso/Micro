using Micro.API.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Micro.API.Data.Configuration;

public class RequisitionCustomFieldConfiguration : IEntityTypeConfiguration<RequisitionCustomField>
{
    public void Configure(EntityTypeBuilder<RequisitionCustomField> builder)
    {
        builder.ToTable("RequisitionCustomFields");
        builder.HasKey(x => new { x.RequisitionId, x.CustomFieldDefinitionId });

        builder.HasOne(x => x.Requisition)
            .WithMany()
            .HasForeignKey(x => x.RequisitionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.CustomFieldDefinition)
            .WithMany()
            .HasForeignKey(x => x.CustomFieldDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
