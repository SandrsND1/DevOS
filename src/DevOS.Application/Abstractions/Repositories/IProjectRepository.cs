using DevOS.Domain.Entities;

namespace DevOS.Application.Abstractions.Repositories
{
    public interface IProjectRepository
    {
        // Теперь с обязательным фильтром по userId
        Task<Project?> GetByIdAsync(
            Guid id, 
            Guid userId, 
            CancellationToken cancellationToken = default);

        Task<int> GetTotalCountAsync(
            Guid userId,
            ProjectStatus? status = null,
            ProjectPriority? priority = null,
            string? search = null,
            CancellationToken cancellationToken = default);

        Task<List<Project>> GetPagedAsync(
            Guid userId,
            int page,
            int pageSize,
            ProjectStatus? status = null,
            ProjectPriority? priority = null,
            string? search = null,
            string? sortBy = null,
            string? sortDirection = null,
            CancellationToken cancellationToken = default);

        Task AddAsync(Project project, CancellationToken cancellationToken = default);
        Task UpdateAsync(Project project, CancellationToken cancellationToken = default);
        Task DeleteAsync(Project project, CancellationToken cancellationToken = default);
    }
}