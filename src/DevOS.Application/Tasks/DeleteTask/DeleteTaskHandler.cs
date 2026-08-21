using DevOS.Application.Exceptions;
using DevOS.Application.Tasks;

namespace DevOS.Application.Tasks.DeleteTask
{
    public class DeleteTaskHandler
    {
        private readonly ITaskRepository _taskRepository;

        public DeleteTaskHandler(ITaskRepository taskRepository)
        {
            _taskRepository = taskRepository;
        }

        public async Task HandleAsync(
            DeleteTaskCommand command,
            CancellationToken cancellationToken = default)
        {
            var task = await _taskRepository.GetByIdAsync(command.TaskId, command.ProjectId, cancellationToken);

            if (task is null)
                throw new TaskNotFoundException(command.TaskId);

            await _taskRepository.DeleteAsync(task, cancellationToken);
        }
    }
}