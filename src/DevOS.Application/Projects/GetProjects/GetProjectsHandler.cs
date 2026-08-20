using DevOS.Application.Projects;

namespace DevOS.Application.Projects.GetProject
{
    public class GetProjectHandler
    {
        private readonly IProjectRepository _projectRepository;

        public GetProjectHandler(IProjectRepository projectRepository)
        {
            _projectRepository = projectRepository;
        }

        public async Task<GetProjectResponse?> HandleAsync(
            GetProjectQuery query,
            CancellationToken cancellationToken = default)
        {
            var project = await _projectRepository.GetByIdAsync(query.Id, cancellationToken);

            if (project is null)
                return null;

            return new GetProjectResponse
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                Status = project.Status,
                Priority = project.Priority,
                Deadline = project.Deadline,
                CreatedAt = project.CreatedAt,
                UpdatedAt = project.UpdatedAt
            };
        }
    }
}