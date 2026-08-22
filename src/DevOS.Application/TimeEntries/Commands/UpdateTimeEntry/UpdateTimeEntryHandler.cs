using DevOS.Application.Abstractions.Repositories;
using DevOS.Application.Abstractions.Services;
using DevOS.Application.TimeEntries.DTOs;
using DevOS.Application.TimeEntries.Mappings;

namespace DevOS.Application.TimeEntries.Commands.UpdateTimeEntry
{
    public class UpdateTimeEntryHandler
    {
        private readonly ITimeEntryRepository _timeEntryRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly ICurrentUserService _currentUserService;

        public UpdateTimeEntryHandler(
            ITimeEntryRepository timeEntryRepository,
            IProjectRepository projectRepository,
            ICurrentUserService currentUserService)
        {
            _timeEntryRepository = timeEntryRepository;
            _projectRepository = projectRepository;
            _currentUserService = currentUserService;
        }

        public async Task<TimeEntryDto?> HandleAsync(
            UpdateTimeEntryCommand command,
            CancellationToken cancellationToken = default)
        {
            var userId = _currentUserService.UserId;
            if (userId == Guid.Empty)
            {
                throw new UnauthorizedAccessException("User must be authenticated.");
            }

            var project = await _projectRepository.GetByIdAsync(command.ProjectId, userId, cancellationToken);
            if (project == null)
            {
                return null;
            }

            var timeEntry = await _timeEntryRepository.GetByIdTrackedAsync(command.Id, cancellationToken);
            if (timeEntry == null || timeEntry.ProjectId != command.ProjectId)
            {
                return null;
            }

            timeEntry.UpdateTimeRange(command.StartedAt, command.EndedAt);
            timeEntry.UpdateDescription(command.Description);
            timeEntry.UpdateTask(command.TaskId);

            await _timeEntryRepository.UpdateAsync(timeEntry, cancellationToken);

            return timeEntry.ToDto();
        }
    }
}