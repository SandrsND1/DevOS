using DevOS.Application.Abstractions.Repositories;
using DevOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DevOS.Infrastructure.Persistence.Repositories
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly DevOsDbContext _dbContext;

        public ProjectRepository(DevOsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Запрос проверяет и Id проекта, и Id его владельца
        public async Task<Project?> GetByIdAsync(
            Guid id, 
            Guid userId, 
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.Projects
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId, cancellationToken);
        }

        public async Task<int> GetTotalCountAsync(
            Guid userId,
            ProjectStatus? status = null,
            ProjectPriority? priority = null,
            string? search = null,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.Projects.AsNoTracking().Where(p => p.UserId == userId);

            if (status.HasValue)
                query = query.Where(p => p.Status == status.Value);

            if (priority.HasValue)
                query = query.Where(p => p.Priority == priority.Value);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p => p.Name.Contains(search) || (p.Description != null && p.Description.Contains(search)));

            return await query.CountAsync(cancellationToken);
        }

        public async Task<List<Project>> GetPagedAsync(
            Guid userId,
            int page,
            int pageSize,
            ProjectStatus? status = null,
            ProjectPriority? priority = null,
            string? search = null,
            string? sortBy = null,
            string? sortDirection = null,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.Projects.AsNoTracking().Where(p => p.UserId == userId);

            if (status.HasValue)
                query = query.Where(p => p.Status == status.Value);

            if (priority.HasValue)
                query = query.Where(p => p.Priority == priority.Value);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p => p.Name.Contains(search) || (p.Description != null && p.Description.Contains(search)));

            var isDescending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            query = sortBy?.ToLower() switch
            {
                "name" => isDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
                "status" => isDescending ? query.OrderByDescending(p => p.Status) : query.OrderBy(p => p.Status),
                "priority" => isDescending ? query.OrderByDescending(p => p.Priority) : query.OrderBy(p => p.Priority),
                _ => isDescending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt)
            };

            return await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(Project project, CancellationToken cancellationToken = default)
        {
            await _dbContext.Projects.AddAsync(project, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(Project project, CancellationToken cancellationToken = default)
        {
            _dbContext.Projects.Update(project);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Project project, CancellationToken cancellationToken = default)
        {
            _dbContext.Projects.Remove(project);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}