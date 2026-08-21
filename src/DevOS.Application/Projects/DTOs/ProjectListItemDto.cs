using DevOS.Domain.Entities;

namespace DevOS.Application.Projects.DTOs
{
    public class ProjectListItemDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public ProjectStatus Status { get; init; }
        public ProjectPriority Priority { get; init; }
        public DateTime? Deadline { get; init; }
        public DateTime CreatedAt { get; init; }
    }
}