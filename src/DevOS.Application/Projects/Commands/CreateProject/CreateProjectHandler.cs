using DevOS.Application.Abstractions.Repositories;
using DevOS.Application.Abstractions.Services;
using DevOS.Application.Projects.DTOs;
using DevOS.Domain.Entities;

namespace DevOS.Application.Projects.Commands.CreateProject
{
    public class CreateProjectHandler
    {
        private readonly IProjectRepository _projectRepository;
        private readonly ICurrentUserService _currentUserService;

        public CreateProjectHandler(
            IProjectRepository projectRepository,
            ICurrentUserService currentUserService)
        {
            _projectRepository = projectRepository;
            _currentUserService = currentUserService;
        }

        public async Task<ProjectDto> HandleAsync(
            CreateProjectCommand command,
            CancellationToken cancellationToken = default)
        {
            // 1. Извлекаем UserId авторизованного пользователя из JWT
            var userId = _currentUserService.UserId;

            if (userId == Guid.Empty)
            {
                throw new UnauthorizedAccessException("User must be authenticated to create a project.");
            }

            // 2. Создаем сущность домена с явной привязкой к владельцу
            var project = new Project(
                userId,
                command.Name,
                command.Priority,
                command.Description,
                command.Deadline);

            // 3. Сохраняем в БД через репозиторий
            await _projectRepository.AddAsync(project, cancellationToken);

            // 4. Возвращаем DTO
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