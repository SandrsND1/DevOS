using DevOS.Application.Abstractions.Repositories;
using DevOS.Application.Abstractions.Services;
using DevOS.Application.Projects.DTOs;

namespace DevOS.Application.Projects.Queries.GetProjects
{
    public class GetProjectsHandler
    {
        private readonly IProjectRepository _projectRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetProjectsHandler(
            IProjectRepository projectRepository,
            ICurrentUserService currentUserService)
        {
            _projectRepository = projectRepository;
            _currentUserService = currentUserService;
        }

        public async Task<PagedResult<ProjectListItemDto>> HandleAsync(
            GetProjectsQuery query,
            CancellationToken cancellationToken = default)
        {
            var userId = _currentUserService.UserId;
            
            if (userId == Guid.Empty)
            {
                throw new UnauthorizedAccessException("User must be authenticated.");
            }

            if (query.Page < 1)
                throw new ArgumentException("Page must be greater than or equal to 1.", nameof(query.Page));

            if (query.PageSize < 1)
                throw new ArgumentException("PageSize must be greater than or equal to 1.", nameof(query.PageSize));

            var totalCount = await _projectRepository.GetTotalCountAsync(
                userId,
                query.Status,
                query.Priority,
                query.Search,
                cancellationToken);

            var projects = await _projectRepository.GetPagedAsync(
                userId,
                query.Page,
                query.PageSize,
                query.Status,
                query.Priority,
                query.Search,
                query.SortBy,
                query.SortDirection,
                cancellationToken);

            var items = projects.Select(p => new ProjectListItemDto
            {
                Id = p.Id,
                Name = p.Name,
                Status = p.Status,
                Priority = p.Priority,
                Deadline = p.Deadline,
                CreatedAt = p.CreatedAt
            }).ToList();

            return new PagedResult<ProjectListItemDto>
            {
                Items = items,
                Page = query.Page,
                PageSize = query.PageSize,
                TotalCount = totalCount
            };
        }
    }
}