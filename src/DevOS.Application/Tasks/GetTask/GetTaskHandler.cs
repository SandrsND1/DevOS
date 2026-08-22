using DevOS.Application.Exceptions;
using DevOS.Application.Abstractions.Repositories;
using DevOS.Application.Abstractions.Services;
using DevOS.Application.Tasks;
using DevOS.Domain.Entities;

namespace DevOS.Application.Tasks.GetTask
{
    public class GetTaskHandler
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetTaskHandler(
            ITaskRepository taskRepository,
            IProjectRepository projectRepository,
            ICurrentUserService currentUserService)
        {
            _taskRepository = taskRepository;
            _projectRepository = projectRepository;
            _currentUserService = currentUserService;
        }

        public async Task<GetTaskResponse?> HandleAsync(
            GetTaskQuery query,
            CancellationToken cancellationToken = default)
        {
            var userId = _currentUserService.UserId;
            if (userId == Guid.Empty)
            {
                throw new UnauthorizedAccessException("User must be authenticated.");
            }

            var project = await _projectRepository.GetByIdAsync(query.ProjectId, userId, cancellationToken);
            if (project == null)
            {
                return null;
            }

            // Исправлено: GetByIdAsync принимает (taskId, projectId, cancellationToken)
            var task = await _taskRepository.GetByIdAsync(query.TaskId, query.ProjectId, cancellationToken);
            if (task == null || task.ProjectId != query.ProjectId)
            {
                return null;
            }

            return new GetTaskResponse
            {
                Id = task.Id,
                ProjectId = task.ProjectId,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                Priority = task.Priority,
                Deadline = task.Deadline,  // Убрано EstimatedMinutes
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt
            };
        }
    }
}