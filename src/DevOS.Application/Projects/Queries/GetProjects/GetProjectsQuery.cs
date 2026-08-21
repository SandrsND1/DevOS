using DevOS.Domain.Entities;

namespace DevOS.Application.Projects.Queries.GetProjects
{
    public class GetProjectsQuery
    {
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 20;
        public ProjectStatus? Status { get; init; }
        public ProjectPriority? Priority { get; init; }
        public string? Search { get; init; }
        public string SortBy { get; init; } = "CreatedAt";
        public string SortDirection { get; init; } = "desc";
    }
}