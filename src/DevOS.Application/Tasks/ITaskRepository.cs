using DevOS.Domain.Entities;

namespace DevOS.Application.Tasks
{
    public interface ITaskRepository
    {
        Task AddAsync(DevTask task, CancellationToken cancellationToken = default);
        Task<List<DevTask>> GetAllByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);
        Task<DevTask?> GetByIdAsync(Guid taskId, Guid projectId, CancellationToken cancellationToken = default);
        Task UpdateAsync(DevTask task, CancellationToken cancellationToken = default);
        Task DeleteAsync(DevTask task, CancellationToken cancellationToken = default);
        Task<int> GetTotalCountAsync(
            Guid projectId,
            DevTaskStatus? status,
            TaskPriority? priority,
            string? search,
            CancellationToken cancellationToken = default);
        Task<List<DevTask>> GetPagedAsync(
            Guid projectId,
            int page,
            int pageSize,
            DevTaskStatus? status,
            TaskPriority? priority,
            string? search,
            string sortBy,
            string sortDirection,
            CancellationToken cancellationToken = default);
    }
}