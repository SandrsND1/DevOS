using DevOS.Application.Abstractions.Repositories;
using DevOS.Application.Exceptions;
using DevOS.Application.Projects.DTOs;

namespace DevOS.Application.Projects.Commands.ChangeProjectStatus
{
    public class ChangeProjectStatusHandler
    {
        private readonly IProjectRepository _projectRepository;

        public ChangeProjectStatusHandler(IProjectRepository projectRepository)
        {
            _projectRepository = projectRepository;
        }

        public async Task<ProjectDto> HandleAsync(
            ChangeProjectStatusCommand command,
            CancellationToken cancellationToken = default)
        {
            var project = await _projectRepository.GetByIdAsync(command.Id, cancellationToken);

            if (project is null)
                throw new ProjectNotFoundException(command.Id);

            project.UpdateStatus(command.Status);

            await _projectRepository.UpdateAsync(project, cancellationToken);

            return new ProjectDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                Status = project.Status,
                Priority = project.Priority,
                Deadline = project.Deadline,
                CreatedAt = project.CreatedAt,
                UpdatedAt = project.UpdatedAt
            };
        }
    }
}