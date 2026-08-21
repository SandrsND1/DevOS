using DevOS.Domain.Entities;

namespace DevOS.Application.Projects.Commands.CreateProject
{
    public class CreateProjectCommand
    {
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
        public ProjectPriority Priority { get; init; }
        public DateTime? Deadline { get; init; }
    }
}