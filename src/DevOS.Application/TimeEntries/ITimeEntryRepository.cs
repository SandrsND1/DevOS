using DevOS.Domain.Entities;

namespace DevOS.Application.TimeEntries
{
    public interface ITimeEntryRepository
    {
        Task AddAsync(TimeEntry timeEntry, CancellationToken cancellationToken = default);
        Task<TimeEntry?> GetByIdAsync(Guid entryId, Guid projectId, CancellationToken cancellationToken = default);
        Task<List<TimeEntry>> GetAllByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);
        Task<List<TimeEntry>> GetAllByTaskIdAsync(Guid taskId, CancellationToken cancellationToken = default);
        Task<List<TimeEntry>> GetByPeriodAsync(Guid projectId, DateTime from, DateTime to, CancellationToken cancellationToken = default);
        Task UpdateAsync(TimeEntry timeEntry, CancellationToken cancellationToken = default);
        Task DeleteAsync(TimeEntry timeEntry, CancellationToken cancellationToken = default);
    }
}