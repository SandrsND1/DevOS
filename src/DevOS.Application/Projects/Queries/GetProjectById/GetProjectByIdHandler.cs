using DevOS.Application.Abstractions.Repositories;
using DevOS.Application.Projects.DTOs;

namespace DevOS.Application.Projects.Queries.GetProjectById
{
    public class GetProjectByIdHandler
    {
        private readonly IProjectRepository _projectRepository;

        public GetProjectByIdHandler(IProjectRepository projectRepository)
        {
            _projectRepository = projectRepository;
        }

        public async Task<ProjectDto?> HandleAsync(
            GetProjectByIdQuery query,
            CancellationToken cancellationToken = default)
        {
            var project = await _projectRepository.GetByIdAsync(query.Id, cancellationToken);

            if (project is null)
                return null;

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