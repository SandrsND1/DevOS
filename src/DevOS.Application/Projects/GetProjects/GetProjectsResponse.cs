using DevOS.Domain.Entities;

namespace DevOS.Application.Projects.GetProject
{
    public class GetProjectResponse
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
        public ProjectStatus Status { get; init; }
        public ProjectPriority Priority { get; init; }
        public DateTime? Deadline { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
    }
}