using DevOS.Application.TimeEntries;
using DevOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DevOS.Infrastructure.Persistence.Repositories
{
    public class TimeEntryRepository : ITimeEntryRepository
    {
        private readonly DevOsDbContext _context;

        public TimeEntryRepository(DevOsDbContext context)
        {
            _context = context;
        }

        public async Task<TimeEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.TimeEntries
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        }

        public async Task<TimeEntry?> GetByIdTrackedAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.TimeEntries
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        }

        public async Task<bool> HasActiveTimerAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.TimeEntries
                .AnyAsync(e => e.ProjectId == userId && e.EndedAt == null, cancellationToken);
        }

        public async Task<List<TimeEntry>> GetFilteredAsync(
            Guid projectId,
            Guid? taskId = null,
            DateTime? from = null,
            DateTime? to = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.TimeEntries
                .AsNoTracking()
                .Where(e => e.ProjectId == projectId);

            if (taskId.HasValue)
            {
                query = query.Where(e => e.TaskId == taskId.Value);
            }

            if (from.HasValue)
            {
                query = query.Where(e => e.StartedAt >= from.Value);
            }

            if (to.HasValue)
            {
                query = query.Where(e => e.StartedAt < to.Value);
            }

            return await query
                .OrderByDescending(e => e.StartedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(TimeEntry entry, CancellationToken cancellationToken = default)
        {
            await _context.TimeEntries.AddAsync(entry, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(TimeEntry entry, CancellationToken cancellationToken = default)
        {
            _context.TimeEntries.Update(entry);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(TimeEntry entry, CancellationToken cancellationToken = default)
        {
            _context.TimeEntries.Remove(entry);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}