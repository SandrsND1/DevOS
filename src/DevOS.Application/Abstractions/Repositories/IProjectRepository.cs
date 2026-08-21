using DevOS.Domain.Entities;

namespace DevOS.Application.Abstractions.Repositories
{
    public interface IProjectRepository
    {
        Task AddAsync(Project project, CancellationToken cancellationToken = default);
        Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<Project>> GetAllAsync(CancellationToken cancellationToken = default);
        Task UpdateAsync(Project project, CancellationToken cancellationToken = default);
        Task DeleteAsync(Project project, CancellationToken cancellationToken = default);
        Task<int> GetTotalCountAsync(
            ProjectStatus? status,
            ProjectPriority? priority,
            string? search,
            CancellationToken cancellationToken = default);
        Task<List<Project>> GetPagedAsync(
            int page,
            int pageSize,
            ProjectStatus? status,
            ProjectPriority? priority,
            string? search,
            string sortBy,
            string sortDirection,
            CancellationToken cancellationToken = default);
    }
}