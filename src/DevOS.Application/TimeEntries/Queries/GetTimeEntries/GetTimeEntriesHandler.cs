using DevOS.Application.Abstractions.Repositories;
using DevOS.Application.Abstractions.Services;
using DevOS.Application.TimeEntries.DTOs;
using DevOS.Application.TimeEntries.Mappings;

namespace DevOS.Application.TimeEntries.Queries.GetTimeEntries
{
    public class GetTimeEntriesHandler
    {
        private readonly ITimeEntryRepository _timeEntryRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetTimeEntriesHandler(
            ITimeEntryRepository timeEntryRepository,
            IProjectRepository projectRepository,
            ICurrentUserService currentUserService)
        {
            _timeEntryRepository = timeEntryRepository;
            _projectRepository = projectRepository;
            _currentUserService = currentUserService;
        }

        public async Task<List<TimeEntryDto>?> HandleAsync(
            GetTimeEntriesQuery query,
            CancellationToken cancellationToken = default)
        {
            var userId = _currentUserService.UserId;
            if (userId == Guid.Empty)
            {
                throw new UnauthorizedAccessException("User must be authenticated.");
            }

            // 1. Security Check: проверяем владение проектом
            var project = await _projectRepository.GetByIdAsync(query.ProjectId, userId, cancellationToken);
            if (project == null)
            {
                return null;
            }

            // 2. Выборка через единый фильтрованный метод в БД
            var entries = await _timeEntryRepository.GetFilteredAsync(
                query.ProjectId,
                query.TaskId,
                query.From,
                query.To,
                cancellationToken
            );

            return entries.ToDtoList().ToList();
        }
    }
}