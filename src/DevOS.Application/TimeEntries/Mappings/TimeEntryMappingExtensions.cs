using DevOS.Application.TimeEntries.DTOs;
using DevOS.Domain.Entities;

namespace DevOS.Application.TimeEntries.Mappings
{
    public static class TimeEntryMappingExtensions
    {
        public static TimeEntryDto ToDto(this TimeEntry entity)
        {
            return new TimeEntryDto
            {
                Id = entity.Id,
                ProjectId = entity.ProjectId,
                TaskId = entity.TaskId,
                StartedAt = entity.StartedAt,
                EndedAt = entity.EndedAt ?? entity.StartedAt, // Если EndedAt null, используем StartedAt
                DurationMinutes = entity.DurationMinutes,
                Description = entity.Description,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }

        public static IEnumerable<TimeEntryDto> ToDtoList(this IEnumerable<TimeEntry> entities)
        {
            return entities.Select(e => e.ToDto());
        }
    }
}