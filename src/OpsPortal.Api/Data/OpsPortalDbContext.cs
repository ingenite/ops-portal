using Microsoft.EntityFrameworkCore;
using OpsPortal.Api.Models;

namespace OpsPortal.Api.Data;

// sealed - can't be inherited by another class
// NewClass : InheritsThisClass
public sealed class OpsPortalDbContext : DbContext {
    // constructor
    // DbContextOptions<TContext> takes config for SQL Srv
    // runs the inherited class' constructor
    public OpsPortalDbContext(DbContextOptions<OpsPortalDbContext> options)
        : base(options) {
        
    }

    public DbSet<WorkItem> WorkItems => Set<WorkItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        var workItem = modelBuilder.Entity<WorkItem>();

        workItem.ToTable("WorkItems");

        workItem.HasKey(item => item.Id);

        workItem.Property(item => item.Title)
            .HasMaxLength(200)
            .IsRequired();

        workItem.Property(item => item.Status)
            .HasMaxLength(20)
            .IsRequired();

        workItem.Property(item => item.CreatedAtUtc)
            .IsRequired();

        workItem.Property(item => item.UpdatedAtUtc)
            .IsRequired();
    }    
}