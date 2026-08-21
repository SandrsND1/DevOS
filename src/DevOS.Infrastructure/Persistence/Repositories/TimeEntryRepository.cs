using DevOS.Application.TimeEntries;
using DevOS.Domain.Entities;
using DevOS.Infrastructure.Persistence;
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

        public async Task AddAsync(TimeEntry timeEntry, CancellationToken cancellationToken = default)
        {
            _context.TimeEntries.Add(timeEntry);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<TimeEntry?> GetByIdAsync(Guid entryId, Guid projectId, CancellationToken cancellationToken = default)
        {
            return await _context.TimeEntries
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == entryId && t.ProjectId == projectId, cancellationToken);
        }

        public async Task<List<TimeEntry>> GetAllByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
        {
            return await _context.TimeEntries
                .AsNoTracking()
                .Where(t => t.ProjectId == projectId)
                .OrderBy(t => t.StartedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<TimeEntry>> GetAllByTaskIdAsync(Guid taskId, CancellationToken cancellationToken = default)
        {
            return await _context.TimeEntries
                .AsNoTracking()
                .Where(t => t.TaskId == taskId)
                .OrderBy(t => t.StartedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<TimeEntry>> GetByPeriodAsync(Guid projectId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
        {
            return await _context.TimeEntries
                .AsNoTracking()
                .Where(t => t.ProjectId == projectId && t.StartedAt >= from && t.StartedAt < to)
                .OrderBy(t => t.StartedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task UpdateAsync(TimeEntry timeEntry, CancellationToken cancellationToken = default)
        {
            _context.TimeEntries.Update(timeEntry);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(TimeEntry timeEntry, CancellationToken cancellationToken = default)
        {
            _context.TimeEntries.Remove(timeEntry);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}