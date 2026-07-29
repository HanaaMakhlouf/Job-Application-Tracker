using Microsoft.EntityFrameworkCore;
using JobApplicationTracker.Models;

namespace JobApplicationTracker.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<JobApplication> JobApplications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<JobApplication>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(50);
            entity.Property(e => e.UserId).HasMaxLength(36).IsRequired();
            entity.Property(e => e.JobTitle).HasMaxLength(500);
            entity.Property(e => e.CompanyName).HasMaxLength(300);
            entity.Property(e => e.JobLink).HasMaxLength(2000);
            entity.Property(e => e.Location).HasMaxLength(300);
            entity.Property(e => e.WorkType).HasMaxLength(50);
            entity.Property(e => e.ApplicationStatus).HasMaxLength(50);
        });
    }
}
