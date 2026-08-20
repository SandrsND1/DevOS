using DevOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevOS.Infrastructure.Persistence.Configurations
{
    public class DevTaskConfiguration : IEntityTypeConfiguration<DevTask>
    {
        public void Configure(EntityTypeBuilder<DevTask> builder)
        {
            builder.ToTable("dev_tasks");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.ProjectId)
                .IsRequired();

            builder.Property(t => t.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(t => t.Description)
                .IsRequired(false);

            builder.Property(t => t.Status)
                .IsRequired();

            builder.Property(t => t.Priority)
                .IsRequired();

            builder.Property(t => t.EstimatedMinutes)
                .IsRequired(false);

            builder.Property(t => t.Deadline)
                .IsRequired(false);

            builder.Property(t => t.CreatedAt)
                .IsRequired();

            builder.Property(t => t.UpdatedAt)
                .IsRequired();

            builder.Property(t => t.CompletedAt)
                .IsRequired(false);

            builder.HasOne<Project>()
                .WithMany()
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(t => t.ProjectId);
        }
    }
}