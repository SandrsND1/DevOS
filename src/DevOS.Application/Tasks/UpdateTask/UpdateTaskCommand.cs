using DevOS.Domain.Entities;

namespace DevOS.Application.Tasks.UpdateTask
{
    public class UpdateTaskCommand
    {
        public Guid TaskId { get; init; }
        public Guid ProjectId { get; init; }
        public string Title { get; init; } = string.Empty;
        public string? Description { get; init; }
        public DevTaskStatus Status { get; init; }
        public TaskPriority Priority { get; init; }
        public int? EstimatedMinutes { get; init; }
        public DateTime? Deadline { get; init; }
    }
}