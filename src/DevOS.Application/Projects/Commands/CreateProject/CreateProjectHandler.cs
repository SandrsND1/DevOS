using DevOS.Application.Abstractions.Repositories;
using DevOS.Application.Projects.DTOs;
using DevOS.Domain.Entities;

namespace DevOS.Application.Projects.Commands.CreateProject
{
    public class CreateProjectHandler
    {
        private readonly IProjectRepository _projectRepository;

        public CreateProjectHandler(IProjectRepository projectRepository)
        {
            _projectRepository = projectRepository;
        }

        public async Task<ProjectDto> HandleAsync(
            CreateProjectCommand command,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(command.Name))
                throw new ArgumentException("Project name cannot be empty or whitespace.", nameof(command.Name));

            var project = new Project(
                command.Name,
                command.Priority,
                command.Description,
                command.Deadline);

            await _projectRepository.AddAsync(project, cancellationToken);

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