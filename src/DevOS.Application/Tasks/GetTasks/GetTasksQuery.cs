using DevOS.Domain.Entities;

namespace DevOS.Application.Tasks.GetTasks
{
    public class GetTasksQuery
    {
        public Guid ProjectId { get; init; }
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 10;
        public string? Status { get; init; }
        public string? Priority { get; init; }
        public string? Search { get; init; }
        public string SortBy { get; init; } = "CreatedAt";
        public string SortDirection { get; init; } = "desc";
    }
}