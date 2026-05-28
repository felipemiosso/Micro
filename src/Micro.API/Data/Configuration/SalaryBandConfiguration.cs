using Micro.API.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Micro.API.Data.Configuration;

public class SalaryBandConfiguration : IEntityTypeConfiguration<SalaryBand>
{
    public void Configure(EntityTypeBuilder<SalaryBand> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.MinAmount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.MaxAmount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.Currency).IsRequired().HasMaxLength(10).HasDefaultValue("USD");
        builder.Property(x => x.IsActive).HasDefaultValue(true);
    }
}
