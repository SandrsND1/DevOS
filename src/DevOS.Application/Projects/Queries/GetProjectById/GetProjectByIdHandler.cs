using DevOS.Application.Abstractions.Repositories;
using DevOS.Application.Abstractions.Services;
using DevOS.Application.Projects.DTOs;

namespace DevOS.Application.Projects.Queries.GetProjectById
{
    public class GetProjectByIdHandler
    {
        private readonly IProjectRepository _projectRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetProjectByIdHandler(
            IProjectRepository projectRepository,
            ICurrentUserService currentUserService)
        {
            _projectRepository = projectRepository;
            _currentUserService = currentUserService;
        }

        public async Task<ProjectDto?> HandleAsync(
            GetProjectByIdQuery query,
            CancellationToken cancellationToken = default)
        {
            var userId = _currentUserService.UserId;

            if (userId == Guid.Empty)
            {
                throw new UnauthorizedAccessException("User must be authenticated.");
            }

            // Ищем проект строго в рамках аккаунта текущего юзера
            var project = await _projectRepository.GetByIdAsync(query.Id, userId, cancellationToken);

            if (project == null)
            {
                return null; // Вернет 404 (NotFound) в контроллере
            }

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