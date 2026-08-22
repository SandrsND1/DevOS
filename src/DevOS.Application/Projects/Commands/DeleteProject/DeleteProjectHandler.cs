using DevOS.Application.Abstractions.Repositories;
using DevOS.Application.Abstractions.Services;

namespace DevOS.Application.Projects.Commands.DeleteProject
{
    public class DeleteProjectHandler
    {
        private readonly IProjectRepository _projectRepository;
        private readonly ICurrentUserService _currentUserService;

        public DeleteProjectHandler(
            IProjectRepository projectRepository,
            ICurrentUserService currentUserService)
        {
            _projectRepository = projectRepository;
            _currentUserService = currentUserService;
        }

        public async Task<bool> HandleAsync(
            DeleteProjectCommand command,
            CancellationToken cancellationToken = default)
        {
            var userId = _currentUserService.UserId;

            if (userId == Guid.Empty)
            {
                throw new UnauthorizedAccessException("User must be authenticated.");
            }

            // Ищем проект по Id и UserId, чтобы избежать случайного или злонамеренного удаления чужого ресурса
            var project = await _projectRepository.GetByIdAsync(command.Id, userId, cancellationToken);

            if (project == null)
            {
                return false; // Вернет 404 в контроллере
            }

            await _projectRepository.DeleteAsync(project, cancellationToken);
            return true;
        }
    }
}