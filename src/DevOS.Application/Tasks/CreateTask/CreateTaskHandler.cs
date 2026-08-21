using DevOS.Application.Exceptions;
using DevOS.Application.Abstractions.Repositories;
using DevOS.Application.Tasks;
using DevOS.Application.Validation;
using DevOS.Domain.Entities;

namespace DevOS.Application.Tasks.CreateTask
{
    public class CreateTaskHandler
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly CreateTaskValidator _validator;

        public CreateTaskHandler(
            ITaskRepository taskRepository,
            IProjectRepository projectRepository,
            CreateTaskValidator validator)
        {
            _taskRepository = taskRepository;
            _projectRepository = projectRepository;
            _validator = validator;
        }

        public async Task<CreateTaskResponse> HandleAsync(
            CreateTaskCommand command,
            CancellationToken cancellationToken = default)
        {
            var validationErrors = _validator.Validate(command);

            if (validationErrors.Count > 0)
                throw new ValidationException(validationErrors);

            var project = await _projectRepository.GetByIdAsync(command.ProjectId, cancellationToken);

            if (project is null)
                throw new ProjectNotFoundException(command.ProjectId);

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
