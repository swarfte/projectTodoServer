using Microsoft.EntityFrameworkCore;
using projectTodoServer.Entities;

namespace projectTodoServer.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Project> Projects => Set<Project>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Project>(entity =>
        {
            entity.ToTable("Projects");

            entity.HasKey(project => project.Id);

            entity.Property(project => project.Id)
                .HasMaxLength(32);

            entity.Property(project => project.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(project => project.UserId)
                .IsRequired()
                .HasMaxLength(32);

            entity.HasIndex(project => project.UserId);
        });
    }
}