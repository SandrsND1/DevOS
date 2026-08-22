using DevOS.Application.Abstractions.Repositories;
using DevOS.Application.Abstractions.Services;
using DevOS.Domain.Entities;

namespace DevOS.Application.Tasks.CreateTask
{
    public class CreateTaskHandler
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly ICurrentUserService _currentUserService;

        public CreateTaskHandler(
            ITaskRepository taskRepository,
            IProjectRepository projectRepository,
            ICurrentUserService currentUserService)
        {
            _taskRepository = taskRepository;
            _projectRepository = projectRepository;
            _currentUserService = currentUserService;
        }

        public async Task<CreateTaskResponse?> HandleAsync(
            CreateTaskCommand command,
            CancellationToken cancellationToken = default)
        {
            var userId = _currentUserService.UserId;
            if (userId == Guid.Empty)
            {
                throw new UnauthorizedAccessException("User must be authenticated.");
            }

            // Проверяем, существует ли проект у ТЕКУЩЕГО пользователя
            var project = await _projectRepository.GetByIdAsync(command.ProjectId, userId, cancellationToken);
            if (project == null)
            {
                return null;
            }

            var task = new DevTask(
                command.ProjectId,
                command.Title,
                command.Priority,
                command.Description,
                command.EstimatedMinutes,
                command.Deadline
            );

            await _taskRepository.AddAsync(task, cancellationToken);

            return new CreateTaskResponse
            {
                Id = task.Id,
                ProjectId = task.ProjectId,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                Priority = task.Priority,
                EstimatedMinutes = task.EstimatedMinutes,
                Deadline = task.Deadline,
                CreatedAt = task.CreatedAt
            };
        }
    }
}