using DevOS.Application.Tasks;
using DevOS.Domain.Entities;
using DevOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DevOS.Infrastructure.Persistence.Repositories
{
    public class TaskRepository : ITaskRepository
    {
        private readonly DevOsDbContext _context;

        public TaskRepository(DevOsDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(DevTask task, CancellationToken cancellationToken = default)
        {
            _context.DevTasks.Add(task);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<DevTask>> GetAllByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
        {
            return await _context.DevTasks
                .AsNoTracking()
                .Where(t => t.ProjectId == projectId)
                .ToListAsync(cancellationToken);
        }

        public async Task<DevTask?> GetByIdAsync(Guid taskId, Guid projectId, CancellationToken cancellationToken = default)
        {
            return await _context.DevTasks
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == taskId && t.ProjectId == projectId, cancellationToken);
        }

        public async Task UpdateAsync(DevTask task, CancellationToken cancellationToken = default)
        {
            _context.DevTasks.Update(task);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(DevTask task, CancellationToken cancellationToken = default)
        {
            _context.DevTasks.Remove(task);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<int> GetTotalCountAsync(
            Guid projectId,
            DevTaskStatus? status,
            TaskPriority? priority,
            string? search,
            CancellationToken cancellationToken = default)
        {
            var query = _context.DevTasks.Where(t => t.ProjectId == projectId);

            if (status.HasValue)
                query = query.Where(t => t.Status == status.Value);

            if (priority.HasValue)
                query = query.Where(t => t.Priority == priority.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(t =>
                    t.Title.Contains(search) ||
                    (t.Description != null && t.Description.Contains(search)));
            }

            return await query.CountAsync(cancellationToken);
        }

        public async Task<List<DevTask>> GetPagedAsync(
            Guid projectId,
            int page,
            int pageSize,
            DevTaskStatus? status,
            TaskPriority? priority,
            string? search,
            string sortBy,
            string sortDirection,
            CancellationToken cancellationToken = default)
        {
            var query = _context.DevTasks.Where(t => t.ProjectId == projectId);

            if (status.HasValue)
                query = query.Where(t => t.Status == status.Value);

            if (priority.HasValue)
                query = query.Where(t => t.Priority == priority.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(t =>
                    t.Title.Contains(search) ||
                    (t.Description != null && t.Description.Contains(search)));
            }

            query = sortBy switch
            {
                "Title" => sortDirection.ToLower() == "asc"
                    ? query.OrderBy(t => t.Title)
                    : query.OrderByDescending(t => t.Title),

                "UpdatedAt" => sortDirection.ToLower() == "asc"
                    ? query.OrderBy(t => t.UpdatedAt)
                    : query.OrderByDescending(t => t.UpdatedAt),

                "Deadline" => sortDirection.ToLower() == "asc"
                    ? query.OrderBy(t => t.Deadline)
                    : query.OrderByDescending(t => t.Deadline),

                "Priority" => sortDirection.ToLower() == "asc"
                    ? query.OrderBy(t => t.Priority)
                    : query.OrderByDescending(t => t.Priority),

                "Status" => sortDirection.ToLower() == "asc"
                    ? query.OrderBy(t => t.Status)
                    : query.OrderByDescending(t => t.Status),

                "EstimatedMinutes" => sortDirection.ToLower() == "asc"
                    ? query.OrderBy(t => t.EstimatedMinutes)
                    : query.OrderByDescending(t => t.EstimatedMinutes),

                _ => sortDirection.ToLower() == "asc"
                    ? query.OrderBy(t => t.CreatedAt)
                    : query.OrderByDescending(t => t.CreatedAt)
            };

            return await query
                .AsNoTracking()
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }
        public async Task<ProjectTaskStatistics> GetTaskStatisticsAsync(
            Guid projectId, 
            CancellationToken cancellationToken = default)
        {
            var tasksQuery = _context.DevTasks
                .AsNoTracking()
                .Where(t => t.ProjectId == projectId);

            var totalTasks = await tasksQuery.CountAsync(cancellationToken);

            var completedTasks = await tasksQuery
                .CountAsync(t => t.Status == DevTaskStatus.Completed, cancellationToken);

            var tasksByStatus = await tasksQuery
                .GroupBy(t => t.Status)
                .ToDictionaryAsync(
                    g => g.Key.ToString(),
                    g => g.Count(),
                   cancellationToken);

            return new ProjectTaskStatistics
            {
                TotalTasks = totalTasks,
                CompletedTasks = completedTasks,
                TasksByStatus = tasksByStatus
            };
        }
    }
}