using IceSync.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IceSync.Infrastructure;

public class IceSyncContext : DbContext
{
    public IceSyncContext()
    {
    }

    public IceSyncContext(DbContextOptions<IceSyncContext> options) : base(options)
    {
    }

    public DbSet<Workflow> Workflows { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Workflow>()
            .HasIndex(x => x.WorkflowId)
            .IsUnique();
    }
}
