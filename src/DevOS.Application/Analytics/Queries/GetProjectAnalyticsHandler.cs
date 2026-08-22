using DevOS.Application.Abstractions.Repositories;
using DevOS.Application.Abstractions.Services;
using DevOS.Application.Analytics.DTOs;
using DevOS.Domain.Entities;

namespace DevOS.Application.Analytics.Queries
{
    public class GetProjectAnalyticsHandler
    {
        private readonly IProjectRepository _projectRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetProjectAnalyticsHandler(
            IProjectRepository projectRepository,
            ICurrentUserService currentUserService)
        {
            _projectRepository = projectRepository;
            _currentUserService = currentUserService;
        }

        public async Task<ProjectAnalyticsDto?> HandleAsync(
            GetProjectAnalyticsQuery query,
            CancellationToken cancellationToken = default)
        {
            var userId = _currentUserService.UserId;

            if (userId == Guid.Empty)
            {
                throw new UnauthorizedAccessException("User must be authenticated.");
            }

            var project = await _projectRepository.GetByIdAsync(query.ProjectId, userId, cancellationToken);

            if (project == null)
            {
                return null;
            }

            return new ProjectAnalyticsDto
            {
                ProjectId = project.Id,
                TotalTasks = 0,
                CompletedTasks = 0,
                CompletionPercentage = 0.0,
                TotalTimeSpentMinutes = 0,
                TasksByStatus = new Dictionary<string, int>()
            };
        }
    }
}