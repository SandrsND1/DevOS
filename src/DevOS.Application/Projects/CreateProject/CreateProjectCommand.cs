using DevOS.Domain.Entities;

namespace DevOS.Application.Projects.CreateProject
{
    public class CreateProjectCommand
    {
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
        public DateTime? Deadline { get; init; }
    }
}