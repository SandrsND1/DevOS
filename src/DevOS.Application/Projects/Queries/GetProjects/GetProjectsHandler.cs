using DevOS.Application.Abstractions.Repositories;
using DevOS.Application.Projects.DTOs;

namespace DevOS.Application.Projects.Queries.GetProjects
{
    public class GetProjectsHandler
    {
        private readonly IProjectRepository _projectRepository;

        public GetProjectsHandler(IProjectRepository projectRepository)
        {
            _projectRepository = projectRepository;
        }

        public async Task<PagedResult<ProjectListItemDto>> HandleAsync(
            GetProjectsQuery query,
            CancellationToken cancellationToken = default)
        {
            if (query.Page < 1)
                throw new ArgumentException("Page must be greater than or equal to 1.", nameof(query.Page));

            if (query.PageSize < 1)
                throw new ArgumentException("PageSize must be greater than or equal to 1.", nameof(query.PageSize));

            var totalCount = await _projectRepository.GetTotalCountAsync(
                query.Status,
                query.Priority,
                query.Search,
                cancellationToken);

            var projects = await _projectRepository.GetPagedAsync(
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