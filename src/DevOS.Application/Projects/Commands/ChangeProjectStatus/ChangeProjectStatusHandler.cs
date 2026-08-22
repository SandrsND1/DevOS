using DevOS.Application.Abstractions.Repositories;
using DevOS.Application.Abstractions.Services;
using DevOS.Application.Projects.DTOs;

namespace DevOS.Application.Projects.Commands.ChangeProjectStatus
{
    public class ChangeProjectStatusHandler
    {
        private readonly IProjectRepository _projectRepository;
        private readonly ICurrentUserService _currentUserService;

        public ChangeProjectStatusHandler(
            IProjectRepository projectRepository,
            ICurrentUserService currentUserService)
        {
            _projectRepository = projectRepository;
            _currentUserService = currentUserService;
        }

        public async Task<ProjectDto?> HandleAsync(
            ChangeProjectStatusCommand command,
            CancellationToken cancellationToken = default)
        {
            var userId = _currentUserService.UserId;

            if (userId == Guid.Empty)
            {
                throw new UnauthorizedAccessException("User must be authenticated.");
            }

            // Получаем проект strictly в рамках аккаунта текущего пользователя
            var project = await _projectRepository.GetByIdAsync(command.Id, userId, cancellationToken);

            if (project == null)
            {
                return null; // Controller вернет 404 (NotFound)
            }

            // Меняем статус через доменный метод с обновлением UpdatedAt
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