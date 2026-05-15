using Micro.API.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Micro.API.Data.Configuration;

public class FeedbackConfiguration : IEntityTypeConfiguration<Feedback>
{
    public void Configure(EntityTypeBuilder<Feedback> builder)
    {
        builder.HasOne(f => f.Application)
            .WithMany(a => a.Feedbacks)
            .HasForeignKey(f => f.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(f => f.AdminId)
            .IsRequired();

        builder.Property(f => f.Notes)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(f => f.Score)
            .IsRequired();
            
        // Note: C# 10 or higher doesn't have CheckConstraint in the builder easily 
        // without .HasCheckConstraint which depends on the provider.
        // We'll rely on API validation but can add it for Postgres:
        // builder.ToTable(t => t.HasCheckConstraint("CK_Feedback_Score", "\"Score\" >= 1 AND \"Score\" <= 5"));
    }
}
