using DevOS.Application.Abstractions.Repositories;
using DevOS.Application.Exceptions;
using DevOS.Application.Tasks;
using DevOS.Application.TimeEntries.DTOs;
using DevOS.Application.Validation;
using DevOS.Domain.Entities;

namespace DevOS.Application.TimeEntries.Commands.CreateTimeEntry
{
    public class CreateTimeEntryHandler
    {
        private readonly ITimeEntryRepository _timeEntryRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly ITaskRepository _taskRepository;
        private readonly CreateTimeEntryValidator _validator;

        public CreateTimeEntryHandler(
            ITimeEntryRepository timeEntryRepository,
            IProjectRepository projectRepository,
            ITaskRepository taskRepository,
            CreateTimeEntryValidator validator)
        {
            _timeEntryRepository = timeEntryRepository;
            _projectRepository = projectRepository;
            _taskRepository = taskRepository;
            _validator = validator;
        }

        public async Task<TimeEntryDto> HandleAsync(
            CreateTimeEntryCommand command,
            CancellationToken cancellationToken = default)
        {
            var validationErrors = _validator.Validate(command);
            if (validationErrors.Count > 0)
                throw new ValidationException(validationErrors);

            var project = await _projectRepository.GetByIdAsync(command.ProjectId, cancellationToken);
            if (project is null)
                throw new ProjectNotFoundException(command.ProjectId);

            if (command.TaskId.HasValue && command.TaskId.Value != Guid.Empty)
            {
                var task = await _taskRepository.GetByIdAsync(command.TaskId.Value, command.ProjectId, cancellationToken);
                if (task is null)
                    throw new TaskNotFoundException(command.TaskId.Value);
            }

            var timeEntry = new TimeEntry(
                command.ProjectId,
                command.StartedAt,
                command.EndedAt,
                command.Description,
                command.TaskId);

            await _timeEntryRepository.AddAsync(timeEntry, cancellationToken);

            return new TimeEntryDto
            {
                Id = timeEntry.Id,
                ProjectId = timeEntry.ProjectId,
                TaskId = timeEntry.TaskId,
                StartedAt = timeEntry.StartedAt,
                EndedAt = timeEntry.EndedAt,
                DurationMinutes = timeEntry.DurationMinutes,
                Description = timeEntry.Description,
                CreatedAt = timeEntry.CreatedAt,
                UpdatedAt = timeEntry.UpdatedAt
            };
        }
    }
}
