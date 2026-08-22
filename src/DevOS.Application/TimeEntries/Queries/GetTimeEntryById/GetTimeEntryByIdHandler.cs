using DevOS.Application.Abstractions.Repositories;
using DevOS.Application.Abstractions.Services;
using DevOS.Application.TimeEntries.DTOs;
using DevOS.Application.TimeEntries.Mappings;

namespace DevOS.Application.TimeEntries.Queries.GetTimeEntryById
{
    public class GetTimeEntryByIdHandler
    {
        private readonly ITimeEntryRepository _timeEntryRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetTimeEntryByIdHandler(
            ITimeEntryRepository timeEntryRepository,
            IProjectRepository projectRepository,
            ICurrentUserService currentUserService)
        {
            _timeEntryRepository = timeEntryRepository;
            _projectRepository = projectRepository;
            _currentUserService = currentUserService;
        }

        public async Task<TimeEntryDto?> HandleAsync(
            GetTimeEntryByIdQuery query,
            CancellationToken cancellationToken = default)
        {
            var userId = _currentUserService.UserId;
            if (userId == Guid.Empty)
            {
                throw new UnauthorizedAccessException("User must be authenticated.");
            }

            // Проверяем владение проектом
            var project = await _projectRepository.GetByIdAsync(query.ProjectId, userId, cancellationToken);
            if (project == null)
            {
                return null;
            }

            var entry = await _timeEntryRepository.GetByIdAsync(query.Id, cancellationToken);
            if (entry == null || entry.ProjectId != query.ProjectId)
            {
                return null;
            }

            return entry.ToDto();
        }
    }
}