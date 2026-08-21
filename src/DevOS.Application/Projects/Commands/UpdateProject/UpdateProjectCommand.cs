using DevOS.Domain.Entities;

namespace DevOS.Application.Projects.Commands.UpdateProject
{
    public class UpdateProjectCommand
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
        public ProjectPriority Priority { get; init; }
        public ProjectStatus Status { get; init; }
        public DateTime? Deadline { get; init; }
    }
}