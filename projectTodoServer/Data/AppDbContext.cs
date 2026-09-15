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

    public DbSet<TaskItem> TaskItems => Set<TaskItem>();

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

        modelBuilder.Entity<TaskItem>(entity =>
            {
                entity.ToTable("Tasks");

                entity.HasKey(task => task.Id);

                entity.Property(task => task.Id).HasMaxLength(32);

                entity.Property(task => task.Name).IsRequired().HasMaxLength(200);

                entity.Property(task => task.ProjectId).IsRequired().HasMaxLength(32);

                entity.Property(task => task.UserId).IsRequired().HasMaxLength(32);

                entity.HasIndex(task => task.ProjectId);

                entity.HasIndex(task => task.PreviousTaskId);

                entity.HasOne(task => task.Project).WithMany(project => project.Tasks)
                    .HasForeignKey(task => task.ProjectId).OnDelete(DeleteBehavior.Cascade);
            }
        );
    }
}