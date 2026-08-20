using DevOS.Domain.Entities;

namespace DevOS.Application.Projects
{
    public interface IProjectRepository
    {
        Task AddAsync(Project project, CancellationToken cancellationToken = default);
        Task<List<Project>> GetAllAsync(CancellationToken cancellationToken = default);
    }
}