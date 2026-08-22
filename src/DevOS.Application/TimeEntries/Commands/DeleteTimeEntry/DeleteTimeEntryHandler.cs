using DevOS.Application.Abstractions.Repositories;
using DevOS.Application.Abstractions.Services;

namespace DevOS.Application.TimeEntries.Commands.DeleteTimeEntry
{
    public class DeleteTimeEntryHandler
    {
        private readonly ITimeEntryRepository _timeEntryRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly ICurrentUserService _currentUserService;

        public DeleteTimeEntryHandler(
            ITimeEntryRepository timeEntryRepository,
            IProjectRepository projectRepository,
            ICurrentUserService currentUserService)
        {
            _timeEntryRepository = timeEntryRepository;
            _projectRepository = projectRepository;
            _currentUserService = currentUserService;
        }

        public async Task<bool> HandleAsync(
            DeleteTimeEntryCommand command,
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
                return false;
            }

            var timeEntry = await _timeEntryRepository.GetByIdAsync(command.Id, cancellationToken);
            if (timeEntry == null || timeEntry.ProjectId != command.ProjectId)
            {
                return false;
            }

            await _timeEntryRepository.DeleteAsync(timeEntry, cancellationToken);
            return true;
        }
    }
}