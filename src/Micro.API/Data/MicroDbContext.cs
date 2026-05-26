using Micro.API.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Micro.API.Data;

public class MicroDbContext : DbContext
{
    public MicroDbContext(DbContextOptions<MicroDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Candidate> Candidates { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<SalaryBand> SalaryBands { get; set; }
    public DbSet<CostCenter> CostCenters { get; set; }
    public DbSet<Requisition> Requisitions { get; set; }
    public DbSet<RequisitionOpening> RequisitionOpenings { get; set; }
    public DbSet<JobPosting> JobPostings { get; set; }
    public DbSet<Application> Applications { get; set; }
    public DbSet<Feedback> Feedbacks { get; set; }
    public DbSet<CustomFieldDefinition> CustomFieldDefinitions { get; set; }
    public DbSet<CustomFieldValue> CustomFieldValues { get; set; }
    public DbSet<RequisitionCustomField> RequisitionCustomFields { get; set; }
    public DbSet<JobPostingCustomField> JobPostingCustomFields { get; set; }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // Normalize all DateTime values to UTC before writing to PostgreSQL's
        // 'timestamp with time zone' columns. Npgsql 6+ rejects Kind=Unspecified.
        configurationBuilder
            .Properties<DateTime>()
            .HaveConversion<UtcDateTimeConverter>();

        configurationBuilder
            .Properties<DateTime?>()
            .HaveConversion<UtcNullableDateTimeConverter>();
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(MicroDbContext).Assembly);
    }
}

/// <summary>
/// Converts a DateTime to UTC kind on write (required by Npgsql for 'timestamp with time zone').
/// On read, marks the returned value as UTC.
/// </summary>
public class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
{
    public UtcDateTimeConverter() : base(
        v => v.Kind == DateTimeKind.Utc ? v : v.ToUniversalTime(),
        v => DateTime.SpecifyKind(v, DateTimeKind.Utc)) { }
}

/// <summary>Nullable variant of <see cref="UtcDateTimeConverter"/>.</summary>
public class UtcNullableDateTimeConverter : ValueConverter<DateTime?, DateTime?>
{
    public UtcNullableDateTimeConverter() : base(
        v => v == null ? v : v.Value.Kind == DateTimeKind.Utc ? v : v.Value.ToUniversalTime(),
        v => v == null ? v : DateTime.SpecifyKind(v.Value, DateTimeKind.Utc)) { }
}
