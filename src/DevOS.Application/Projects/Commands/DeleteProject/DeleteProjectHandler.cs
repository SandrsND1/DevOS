using DevOS.Application.Abstractions.Repositories;
using DevOS.Application.Exceptions;

namespace DevOS.Application.Projects.Commands.DeleteProject
{
    public class DeleteProjectHandler
    {
        private readonly IProjectRepository _projectRepository;

        public DeleteProjectHandler(IProjectRepository projectRepository)
        {
            _projectRepository = projectRepository;
        }

        public async Task HandleAsync(
            DeleteProjectCommand command,
            CancellationToken cancellationToken = default)
        {
            var project = await _projectRepository.GetByIdAsync(command.Id, cancellationToken);

            if (project is null)
                throw new ProjectNotFoundException(command.Id);

            await _projectRepository.DeleteAsync(project, cancellationToken);
        }
    }
}