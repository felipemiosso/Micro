using Micro.API.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Micro.API.Data;

public class MicroDbContext : DbContext
{
    public MicroDbContext(DbContextOptions<MicroDbContext> options) : base(options) { }

    public DbSet<AppUser> Users { get; set; }
    public DbSet<Requisition> Requisitions { get; set; }
    public DbSet<JobPosting> JobPostings { get; set; }
    public DbSet<Application> Applications { get; set; }
    public DbSet<Feedback> Feedbacks { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(MicroDbContext).Assembly);
    }
}
