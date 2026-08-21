using DevOS.Domain.Entities;

namespace DevOS.Application.Tasks.GetTasks
{
    public class GetTasksResponse
    {
        public List<TaskItem> Items { get; init; } = new();
        public int Page { get; init; }
        public int PageSize { get; init; }
        public int TotalCount { get; init; }
        public int TotalPages { get; init; }
    }

    public class TaskItem
    {
        public Guid Id { get; init; }
        public Guid ProjectId { get; init; }
        public string Title { get; init; } = string.Empty;
        public string? Description { get; init; }
        public DevTaskStatus Status { get; init; }
        public TaskPriority Priority { get; init; }
        public int? EstimatedMinutes { get; init; }
        public DateTime? Deadline { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
    }
}