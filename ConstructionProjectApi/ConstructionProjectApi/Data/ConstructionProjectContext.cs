using Microsoft.EntityFrameworkCore;
using ConstructionProjectApi.Models;

namespace ConstructionProjectApi.Data
{
    public class ConstructionProjectContext : DbContext
    {
        public ConstructionProjectContext(DbContextOptions<ConstructionProjectContext> options) : base(options)
        {
        }

        public DbSet<ConstructionProject> ConstructionProjects { get; set; }
        public DbSet<ProjectTask> ProjectTasks { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Resource> Resources { get; set; }
        public DbSet<ResourceUsage> ResourceUsages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProjectTask>()
                .HasOne(pt => pt.Project)
                .WithMany()
                .HasForeignKey(pt => pt.ProjectId);

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Project)
                .WithMany()
                .HasForeignKey(e => e.ProjectId);

            modelBuilder.Entity<ResourceUsage>()
                .HasOne(ru => ru.Task)
                .WithMany()
                .HasForeignKey(ru => ru.TaskId);

            modelBuilder.Entity<ResourceUsage>()
                .HasOne(ru => ru.Resource)
                .WithMany()
                .HasForeignKey(ru => ru.ResourceId);
        }
    }
}
