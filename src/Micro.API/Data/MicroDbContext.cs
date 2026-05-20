using Micro.API.Data.Models;
using Microsoft.EntityFrameworkCore;

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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(MicroDbContext).Assembly);
    }
}
