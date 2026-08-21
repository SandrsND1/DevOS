using DevOS.Application.Abstractions.Repositories;
using DevOS.Application.Exceptions;
using DevOS.Application.Projects.DTOs;

namespace DevOS.Application.Projects.Commands.UpdateProject
{
    public class UpdateProjectHandler
    {
        private readonly IProjectRepository _projectRepository;

        public UpdateProjectHandler(IProjectRepository projectRepository)
        {
            _projectRepository = projectRepository;
        }

        public async Task<ProjectDto> HandleAsync(
            UpdateProjectCommand command,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(command.Name))
                throw new ArgumentException("Project name cannot be empty or whitespace.", nameof(command.Name));

            var project = await _projectRepository.GetByIdAsync(command.Id, cancellationToken);

            if (project is null)
                throw new ProjectNotFoundException(command.Id);

            project.UpdateName(command.Name);
            project.UpdateDescription(command.Description);
            project.UpdatePriority(command.Priority);
            project.UpdateStatus(command.Status);
            project.UpdateDeadline(command.Deadline);

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