using DevOS.Application.Projects;
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
    }
}