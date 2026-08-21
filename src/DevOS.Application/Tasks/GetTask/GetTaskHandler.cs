using DevOS.Application.Exceptions;
using DevOS.Application.Tasks;

namespace DevOS.Application.Tasks.GetTask
{
    public class GetTaskHandler
    {
        private readonly ITaskRepository _taskRepository;

        public GetTaskHandler(ITaskRepository taskRepository)
        {
            _taskRepository = taskRepository;
        }

        public async Task<GetTaskResponse> HandleAsync(
            GetTaskQuery query,
            CancellationToken cancellationToken = default)
        {
            var task = await _taskRepository.GetByIdAsync(query.TaskId, query.ProjectId, cancellationToken);

            if (task is null)
                throw new TaskNotFoundException(query.TaskId);

            return new GetTaskResponse
            {
                Id = task.Id,
                ProjectId = task.ProjectId,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                Priority = task.Priority,
                EstimatedMinutes = task.EstimatedMinutes,
                Deadline = task.Deadline,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt
            };
        }
    }
}