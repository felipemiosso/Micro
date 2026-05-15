using Micro.API.Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Micro.API.Data;

public class MicroDbContext : IdentityDbContext
{
    public MicroDbContext(DbContextOptions<MicroDbContext> options) : base(options) { }

    public DbSet<Requisition> Requisitions { get; set; }
    public DbSet<JobPosting> JobPostings { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(MicroDbContext).Assembly);
    }
}
