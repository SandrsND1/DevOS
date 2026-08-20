using DevOS.Application.Projects;

namespace DevOS.Application.Projects.GetProjects
{
    public class GetProjectsHandler
    {
        private readonly IProjectRepository _projectRepository;

        public GetProjectsHandler(IProjectRepository projectRepository)
        {
            _projectRepository = projectRepository;
        }

        public async Task<List<GetProjectsResponse>> HandleAsync(
            GetProjectsQuery query,
            CancellationToken cancellationToken = default)
        {
            var projects = await _projectRepository.GetAllAsync(cancellationToken);

            return projects.Select(p => new GetProjectsResponse
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Status = p.Status,
                Priority = p.Priority,
                Deadline = p.Deadline,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            }).ToList();
        }
    }
}