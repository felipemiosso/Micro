using Micro.API.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Micro.API.Data.Configuration;

public class CustomFieldDefinitionConfiguration : IEntityTypeConfiguration<CustomFieldDefinition>
{
    public void Configure(EntityTypeBuilder<CustomFieldDefinition> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Label).IsRequired().HasMaxLength(200);
        builder.Property(x => x.HelpText).HasMaxLength(500);
        builder.Property(x => x.ValidationJson).HasColumnType("jsonb");
        builder.Property(x => x.TargetEntity).HasConversion<string>();
        builder.Property(x => x.FieldType).HasConversion<string>();

        // Queries always filter by entity + active status + order
        builder.HasIndex(x => new { x.TargetEntity, x.IsDisabled, x.Order });
    }
}
