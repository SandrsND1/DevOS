using DevOS.Domain.Entities;

namespace DevOS.Application.Tasks.GetTask
{
    public class GetTaskResponse
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DevTaskStatus Status { get; set; }
        public TaskPriority Priority { get; set; }
        public DateTime? Deadline { get; set; } // <--- Переименовали в Deadline для единообразия
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}