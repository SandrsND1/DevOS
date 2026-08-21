using DevOS.Application.Exceptions;
using DevOS.Application.TimeEntries.DTOs;

namespace DevOS.Application.TimeEntries.Queries.GetTimeEntryById
{
    public class GetTimeEntryByIdHandler
    {
        private readonly ITimeEntryRepository _timeEntryRepository;

        public GetTimeEntryByIdHandler(ITimeEntryRepository timeEntryRepository)
        {
            _timeEntryRepository = timeEntryRepository;
        }

        public async Task<TimeEntryDto> HandleAsync(
            GetTimeEntryByIdQuery query,
            CancellationToken cancellationToken = default)
        {
            var timeEntry = await _timeEntryRepository.GetByIdAsync(query.EntryId, query.ProjectId, cancellationToken);
            if (timeEntry is null)
                throw new TimeEntryNotFoundException(query.EntryId);

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
