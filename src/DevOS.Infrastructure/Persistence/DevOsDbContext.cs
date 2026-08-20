using DevOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DevOS.Infrastructure.Persistence
{
    public class DevOsDbContext : DbContext
    {
        public DevOsDbContext(DbContextOptions<DevOsDbContext> options) 
            : base(options)
        {
        }

        public DbSet<Project> Projects { get; set; }
        public DbSet<DevTask> DevTasks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(DevOsDbContext).Assembly);
        }
    }
}