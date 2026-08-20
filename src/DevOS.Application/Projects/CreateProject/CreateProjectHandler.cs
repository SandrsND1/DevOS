using DevOS.Domain.Entities;

namespace DevOS.Application.Projects.CreateProject
{
    public class CreateProjectHandler
    {
        private readonly IProjectRepository _projectRepository;

        public CreateProjectHandler(IProjectRepository projectRepository)
        {
            _projectRepository = projectRepository;
        }

        public async Task<CreateProjectResponse> HandleAsync(
            CreateProjectCommand command,
            CancellationToken cancellationToken = default)
        {
            var project = new Project(
                command.Name,
                command.Description,
                command.Deadline
            );

            await _projectRepository.AddAsync(project, cancellationToken);

            return new CreateProjectResponse
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                Status = project.Status,
                Priority = project.Priority,
                Deadline = project.Deadline,
                CreatedAt = project.CreatedAt
            };
        }
    }
}