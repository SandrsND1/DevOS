using DevOS.Application.Abstractions.Repositories;
using DevOS.Application.Abstractions.Services;
using DevOS.Application.Projects.DTOs;

namespace DevOS.Application.Projects.Commands.UpdateProject
{
    public class UpdateProjectHandler
    {
        private readonly IProjectRepository _projectRepository;
        private readonly ICurrentUserService _currentUserService;

        public UpdateProjectHandler(
            IProjectRepository projectRepository,
            ICurrentUserService currentUserService)
        {
            _projectRepository = projectRepository;
            _currentUserService = currentUserService;
        }

        public async Task<ProjectDto?> HandleAsync(
            UpdateProjectCommand command,
            CancellationToken cancellationToken = default)
        {
            var userId = _currentUserService.UserId;

            if (userId == Guid.Empty)
            {
                throw new UnauthorizedAccessException("User must be authenticated.");
            }

            // Ищем проект строго в рамках аккаунта текущего пользователя
            var project = await _projectRepository.GetByIdAsync(command.Id, userId, cancellationToken);

            if (project == null)
            {
                return null; // Вернет 404 (NotFound) в контроллере
            }

            // Обновляем состояние через методы доменной сущности
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