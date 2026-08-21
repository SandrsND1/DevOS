using DevOS.Application.Exceptions;
using DevOS.Application.Tasks;
using DevOS.Application.Validation;

namespace DevOS.Application.Tasks.UpdateTask
{
    public class UpdateTaskHandler
    {
        private readonly ITaskRepository _taskRepository;
        private readonly UpdateTaskValidator _validator;

        public UpdateTaskHandler(
            ITaskRepository taskRepository,
            UpdateTaskValidator validator)
        {
            _taskRepository = taskRepository;
            _validator = validator;
        }

        public async Task<UpdateTaskResponse> HandleAsync(
            UpdateTaskCommand command,
            CancellationToken cancellationToken = default)
        {
            var validationErrors = _validator.Validate(command);

            if (validationErrors.Count > 0)
                throw new ValidationException(validationErrors);

            var task = await _taskRepository.GetByIdAsync(command.TaskId, command.ProjectId, cancellationToken);

            if (task is null)
                throw new TaskNotFoundException(command.TaskId);

            task.UpdateTitle(command.Title);
            task.UpdateDescription(command.Description);
            task.UpdateStatus(command.Status);
            task.UpdatePriority(command.Priority);
            task.UpdateEstimatedMinutes(command.EstimatedMinutes);
            task.UpdateDeadline(command.Deadline);

            await _taskRepository.UpdateAsync(task, cancellationToken);

            return new UpdateTaskResponse
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