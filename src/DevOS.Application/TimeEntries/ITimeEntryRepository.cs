using DevOS.Domain.Entities;

namespace DevOS.Application.TimeEntries
{
    public interface ITimeEntryRepository
    {
        Task<TimeEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task<TimeEntry?> GetByIdTrackedAsync(Guid id, CancellationToken cancellationToken = default);

        Task<bool> HasActiveTimerAsync(Guid userId, CancellationToken cancellationToken = default);

        Task<List<TimeEntry>> GetFilteredAsync(
            Guid projectId,
            Guid? taskId = null,
            DateTime? from = null,
            DateTime? to = null,
            CancellationToken cancellationToken = default);

        Task AddAsync(TimeEntry entry, CancellationToken cancellationToken = default);
        Task UpdateAsync(TimeEntry entry, CancellationToken cancellationToken = default);
        Task DeleteAsync(TimeEntry entry, CancellationToken cancellationToken = default);
    }
}