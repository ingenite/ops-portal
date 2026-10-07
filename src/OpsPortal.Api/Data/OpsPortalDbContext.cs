using Microsoft.EntityFrameworkCore;

using OpsPortal.Api.Models;

namespace OpsPortal.Api.Data;

// sealed - can't be inherited by another class
// NewClass : InheritsThisClass
public sealed class OpsPortalDbContext : DbContext
{
    // constructor
    // DbContextOptions<TContext> takes config for SQL Srv
    // runs the inherited class' constructor
    public OpsPortalDbContext(DbContextOptions<OpsPortalDbContext> options)
        : base(options)
    {

    }

    public DbSet<WorkItem> WorkItems => Set<WorkItem>();
    public DbSet<Application> Applications => Set<Application>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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

        workItem.HasOne<Application>()
            .WithMany()
            .HasForeignKey(item => item.ApplicationId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // Atlassian Assets - Application
        var application = modelBuilder.Entity<Application>();

        application.ToTable("Applications");
        application.HasKey(app => app.Id);
        application.HasIndex(app => new { app.WorkspaceId, app.ObjectId })
            .IsUnique();

        application.Property(app => app.WorkspaceId)
            .HasMaxLength(100)
            .IsRequired();

        application.Property(app => app.ObjectId)
            .HasMaxLength(100)
            .IsRequired();

        application.Property(app => app.Key)
            .HasMaxLength(100)
            .IsRequired();

        application.Property(app => app.Name)
            .HasMaxLength(255)
            .IsRequired();

        application.Property(app => app.DeploymentMethod)
            .HasMaxLength(255)
            .IsRequired(false);

        application.Property(app => app.Status)
            .HasMaxLength(100)
            .IsRequired(false);

        application.Property(app => app.LastSyncedAtUtc)
            .IsRequired();
    }
}