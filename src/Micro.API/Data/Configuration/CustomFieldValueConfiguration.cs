using Micro.API.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Micro.API.Data.Configuration;

public class CustomFieldValueConfiguration : IEntityTypeConfiguration<CustomFieldValue>
{
    public void Configure(EntityTypeBuilder<CustomFieldValue> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Value).IsRequired().HasMaxLength(10000);
        builder.Property(x => x.TargetEntity).HasConversion<string>();

        // Primary lookup pattern: all values for a given entity record
        builder.HasIndex(x => new { x.EntityId, x.TargetEntity });
        // Secondary: all values for a given field definition (count queries, rule-change pre-flight)
        builder.HasIndex(x => x.CustomFieldDefinitionId);

        builder.HasOne(x => x.Definition)
            .WithMany(x => x.Values)
            .HasForeignKey(x => x.CustomFieldDefinitionId)
            .OnDelete(DeleteBehavior.Restrict); // never cascade-delete values
    }
}
