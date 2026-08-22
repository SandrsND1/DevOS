using DevOS.Application.Abstractions.Repositories;
using DevOS.Application.Abstractions.Services;
using DevOS.Application.TimeEntries.DTOs;
using DevOS.Application.TimeEntries.Mappings;
using DevOS.Domain.Entities;

namespace DevOS.Application.TimeEntries.Commands.CreateTimeEntry
{
    public class CreateTimeEntryHandler
    {
        private readonly ITimeEntryRepository _timeEntryRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly ICurrentUserService _currentUserService;

        public CreateTimeEntryHandler(
            ITimeEntryRepository timeEntryRepository,
            IProjectRepository projectRepository,
            ICurrentUserService currentUserService)
        {
            _timeEntryRepository = timeEntryRepository;
            _projectRepository = projectRepository;
            _currentUserService = currentUserService;
        }

        public async Task<TimeEntryDto?> HandleAsync(
            CreateTimeEntryCommand command,
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

            // Если передается запись без EndedAt (активный таймер), проверяем незавершенные
            if (!command.EndedAt.HasValue)
            {
                var hasActive = await _timeEntryRepository.HasActiveTimerAsync(userId, cancellationToken);
                if (hasActive)
                {
                    throw new InvalidOperationException("User already has an active timer running.");
                }
            }

            var timeEntry = new TimeEntry(
                command.ProjectId,
                command.StartedAt,
                command.EndedAt,
                command.Description,
                command.TaskId
            );

            await _timeEntryRepository.AddAsync(timeEntry, cancellationToken);

            return timeEntry.ToDto();
        }
    }
}