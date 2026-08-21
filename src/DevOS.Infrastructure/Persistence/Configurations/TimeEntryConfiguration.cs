using DevOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevOS.Infrastructure.Persistence.Configurations
{
    public class TimeEntryConfiguration : IEntityTypeConfiguration<TimeEntry>
    {
        public void Configure(EntityTypeBuilder<TimeEntry> builder)
        {
            builder.ToTable("time_entries");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.ProjectId)
                .IsRequired();

            builder.Property(t => t.TaskId)
                .IsRequired(false);

            builder.Property(t => t.StartedAt)
                .IsRequired();

            builder.Property(t => t.EndedAt)
                .IsRequired();

            builder.Property(t => t.DurationMinutes)
                .IsRequired();

            builder.Property(t => t.Description)
                .IsRequired(false)
                .HasMaxLength(2000);

            builder.Property(t => t.CreatedAt)
                .IsRequired();

            builder.Property(t => t.UpdatedAt)
                .IsRequired();

            // FK на Project: при удалении проекта — удаляем все TimeEntry
            builder.HasOne<Project>()
                .WithMany()
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // FK на DevTask: при удалении задачи — отвязываем TaskId (SET NULL)
            builder.HasOne<DevTask>()
                .WithMany()
                .HasForeignKey(t => t.TaskId)
                .OnDelete(DeleteBehavior.SetNull);

            // Индексы для частых запросов
            builder.HasIndex(t => t.ProjectId);
            builder.HasIndex(t => t.TaskId);
            builder.HasIndex(t => t.StartedAt);
        }
    }
}