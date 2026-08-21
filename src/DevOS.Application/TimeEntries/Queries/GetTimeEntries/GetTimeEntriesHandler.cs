using DevOS.Application.Abstractions.Repositories;
using DevOS.Application.Exceptions;
using DevOS.Application.TimeEntries.DTOs;
using DevOS.Application.Validation;
using DevOS.Domain.Entities;

namespace DevOS.Application.TimeEntries.Queries.GetTimeEntries
{
    public class GetTimeEntriesHandler
    {
        private readonly ITimeEntryRepository _timeEntryRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly GetTimeEntriesValidator _validator;

        public GetTimeEntriesHandler(
            ITimeEntryRepository timeEntryRepository,
            IProjectRepository projectRepository,
            GetTimeEntriesValidator validator)
        {
            _timeEntryRepository = timeEntryRepository;
            _projectRepository = projectRepository;
            _validator = validator;
        }

        public async Task<List<TimeEntryDto>> HandleAsync(
            GetTimeEntriesQuery query,
            CancellationToken cancellationToken = default)
        {
            var validationErrors = _validator.Validate(query);
            if (validationErrors.Count > 0)
                throw new ValidationException(validationErrors);

            var project = await _projectRepository.GetByIdAsync(query.ProjectId, cancellationToken);
            if (project is null)
                throw new ProjectNotFoundException(query.ProjectId);

            List<TimeEntry> entries;

            if (query.From.HasValue && query.To.HasValue)
            {
                entries = await _timeEntryRepository.GetByPeriodAsync(
                    query.ProjectId,
                    query.From.Value,
                    query.To.Value,
                    cancellationToken);

                if (query.TaskId.HasValue)
                {
                    entries = entries.Where(e => e.TaskId == query.TaskId.Value).ToList();
                }
            }
            else if (query.TaskId.HasValue)
            {
                entries = await _timeEntryRepository.GetAllByTaskIdAsync(query.TaskId.Value, cancellationToken);
                entries = entries.Where(e => e.ProjectId == query.ProjectId).ToList();
            }
            else
            {
                entries = await _timeEntryRepository.GetAllByProjectIdAsync(query.ProjectId, cancellationToken);
            }

            return entries.Select(e => new TimeEntryDto
            {
                Id = e.Id,
                ProjectId = e.ProjectId,
                TaskId = e.TaskId,
                StartedAt = e.StartedAt,
                EndedAt = e.EndedAt,
                DurationMinutes = e.DurationMinutes,
                Description = e.Description,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt
            }).ToList();
        }
    }
}
