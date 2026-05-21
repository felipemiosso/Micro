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

        builder.HasOne(a => a.Candidate)
            .WithMany(c => c.Applications)
            .HasForeignKey(a => a.CandidateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(a => new { a.JobPostingId, a.CandidateId })
            .IsUnique();

        builder.HasOne(a => a.RequisitionOpening)
            .WithMany()
            .HasForeignKey(a => a.RequisitionOpeningId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(a => a.Status);

        builder.OwnsOne(a => a.Interview, i =>
        {
            i.Property(p => p.ScheduledDate).HasColumnName("InterviewScheduledDate");
            i.Property(p => p.InterviewerName).HasColumnName("InterviewerName").HasMaxLength(100);
        });

        builder.OwnsOne(a => a.Offer, o =>
        {
            o.Property(p => p.ProposedSalary).HasColumnName("OfferProposedSalary").HasColumnType("decimal(18,2)");
            o.Property(p => p.TargetStartDate).HasColumnName("OfferTargetStartDate");
            o.Property(p => p.Deadline).HasColumnName("OfferDeadline");
        });
    }
}
