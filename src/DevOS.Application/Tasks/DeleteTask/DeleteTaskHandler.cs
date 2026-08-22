using DevOS.Application.Abstractions.Repositories;
using DevOS.Application.Abstractions.Services;
using DevOS.Application.Exceptions;
using DevOS.Application.Tasks;

namespace DevOS.Application.Tasks.DeleteTask
{
    public class DeleteTaskHandler
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly ICurrentUserService _currentUserService;

        public DeleteTaskHandler(
            ITaskRepository taskRepository,
            IProjectRepository projectRepository,
            ICurrentUserService currentUserService)
        {
            _taskRepository = taskRepository;
            _projectRepository = projectRepository;
            _currentUserService = currentUserService;
        }

        public async Task HandleAsync(
            DeleteTaskCommand command,
            CancellationToken cancellationToken = default)
        {
            var userId = _currentUserService.UserId;
            if (userId == Guid.Empty)
            {
                throw new UnauthorizedAccessException("User must be authenticated.");
            }

            // Проверяем, что проект принадлежит пользователю
            var project = await _projectRepository.GetByIdAsync(command.ProjectId, userId, cancellationToken);
            if (project == null)
            {
                throw new ProjectNotFoundException(command.ProjectId);
            }

            var task = await _taskRepository.GetByIdAsync(command.TaskId, command.ProjectId, cancellationToken);

            if (task is null)
                throw new TaskNotFoundException(command.TaskId);

            await _taskRepository.DeleteAsync(task, cancellationToken);
        }
    }
}