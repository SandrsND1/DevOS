using DevOS.Domain.Entities;

namespace DevOS.Application.Projects.Commands.ChangeProjectStatus
{
    public class ChangeProjectStatusCommand
    {
        public Guid Id { get; init; }
        public ProjectStatus Status { get; init; }
    }
}