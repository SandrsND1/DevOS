using DevOS.Application.Exceptions;
using DevOS.Application.Tasks;
using DevOS.Application.TimeEntries.DTOs;
using DevOS.Application.Validation;

namespace DevOS.Application.TimeEntries.Commands.UpdateTimeEntry
{
    public class UpdateTimeEntryHandler
    {
        private readonly ITimeEntryRepository _timeEntryRepository;
        private readonly ITaskRepository _taskRepository;
        private readonly UpdateTimeEntryValidator _validator;

        public UpdateTimeEntryHandler(
            ITimeEntryRepository timeEntryRepository,
            ITaskRepository taskRepository,
            UpdateTimeEntryValidator validator)
        {
            _timeEntryRepository = timeEntryRepository;
            _taskRepository = taskRepository;
            _validator = validator;
        }

        public async Task<TimeEntryDto> HandleAsync(
            UpdateTimeEntryCommand command,
            CancellationToken cancellationToken = default)
        {
            var validationErrors = _validator.Validate(command);
            if (validationErrors.Count > 0)
                throw new ValidationException(validationErrors);

            var timeEntry = await _timeEntryRepository.GetByIdAsync(command.EntryId, command.ProjectId, cancellationToken);
            if (timeEntry is null)
                throw new TimeEntryNotFoundException(command.EntryId);

            if (command.TaskId.HasValue && command.TaskId.Value != Guid.Empty)
            {
                var task = await _taskRepository.GetByIdAsync(command.TaskId.Value, command.ProjectId, cancellationToken);
                if (task is null)
                    throw new TaskNotFoundException(command.TaskId.Value);
            }

            timeEntry.UpdateTimeRange(command.StartedAt, command.EndedAt);
            timeEntry.UpdateDescription(command.Description);
            timeEntry.UpdateTask(command.TaskId);

            await _timeEntryRepository.UpdateAsync(timeEntry, cancellationToken);

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
