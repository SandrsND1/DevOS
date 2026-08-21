using DevOS.Application.Abstractions.Repositories;
using DevOS.Domain.Entities;
using DevOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DevOS.Infrastructure.Persistence.Repositories
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly DevOsDbContext _context;

        public ProjectRepository(DevOsDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Project project, CancellationToken cancellationToken = default)
        {
            _context.Projects.Add(project);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<Project>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Projects
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Projects
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public async Task UpdateAsync(Project project, CancellationToken cancellationToken = default)
        {
            _context.Projects.Update(project);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Project project, CancellationToken cancellationToken = default)
        {
            _context.Projects.Remove(project);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<int> GetTotalCountAsync(
            ProjectStatus? status,
            ProjectPriority? priority,
            string? search,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Projects.AsQueryable();

            if (status.HasValue)
                query = query.Where(p => p.Status == status.Value);

            if (priority.HasValue)
                query = query.Where(p => p.Priority == priority.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p =>
                    p.Name.Contains(search) ||
                    (p.Description != null && p.Description.Contains(search)));
            }

            return await query.CountAsync(cancellationToken);
        }

        public async Task<List<Project>> GetPagedAsync(
            int page,
            int pageSize,
            ProjectStatus? status,
            ProjectPriority? priority,
            string? search,
            string sortBy,
            string sortDirection,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Projects.AsQueryable();

            if (status.HasValue)
                query = query.Where(p => p.Status == status.Value);

            if (priority.HasValue)
                query = query.Where(p => p.Priority == priority.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p =>
                    p.Name.Contains(search) ||
                    (p.Description != null && p.Description.Contains(search)));
            }

            query = sortBy switch
            {
                "Name" => sortDirection.ToLower() == "asc"
                    ? query.OrderBy(p => p.Name)
                    : query.OrderByDescending(p => p.Name),

                "UpdatedAt" => sortDirection.ToLower() == "asc"
                    ? query.OrderBy(p => p.UpdatedAt)
                    : query.OrderByDescending(p => p.UpdatedAt),

                "Deadline" => sortDirection.ToLower() == "asc"
                    ? query.OrderBy(p => p.Deadline)
                    : query.OrderByDescending(p => p.Deadline),

                "Priority" => sortDirection.ToLower() == "asc"
                    ? query.OrderBy(p => p.Priority)
                    : query.OrderByDescending(p => p.Priority),

                "Status" => sortDirection.ToLower() == "asc"
                    ? query.OrderBy(p => p.Status)
                    : query.OrderByDescending(p => p.Status),

                _ => sortDirection.ToLower() == "asc"
                    ? query.OrderBy(p => p.CreatedAt)
                    : query.OrderByDescending(p => p.CreatedAt)
            };

            return await query
                .AsNoTracking()
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }
    }
}