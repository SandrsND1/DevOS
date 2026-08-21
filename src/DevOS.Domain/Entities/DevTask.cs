namespace DevOS.Domain.Entities
{
    public class DevTask
    {
        public Guid Id { get; private set; }
        public Guid ProjectId { get; private set; }
        public string Title { get; private set; } = string.Empty;
        public string? Description { get; private set; }
        public DevTaskStatus Status { get; private set; }
        public TaskPriority Priority { get; private set; }
        public int? EstimatedMinutes { get; private set; }
        public DateTime? Deadline { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }
        public DateTime? CompletedAt { get; private set; }

        // Parameterless constructor for EF Core
        private DevTask() { }

        public DevTask(
            Guid projectId,
            string title,
            TaskPriority priority,
            string? description = null,
            int? estimatedMinutes = null,
            DateTime? deadline = null)
        {
            if (projectId == Guid.Empty)
                throw new ArgumentException("Project ID cannot be empty.", nameof(projectId));

            Id = Guid.NewGuid();
            ProjectId = projectId;
            SetTitle(title);
            SetDescription(description);
            Priority = priority;
            SetEstimatedMinutes(estimatedMinutes);
            Status = DevTaskStatus.Todo;
            Deadline = deadline;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            CompletedAt = null;
        }

        public void UpdateTitle(string title)
        {
            SetTitle(title);
            UpdateTimestamp();
        }

        public void UpdateDescription(string? description)
        {
            SetDescription(description);
            UpdateTimestamp();
        }

        public void UpdatePriority(TaskPriority priority)
        {
            Priority = priority;
            UpdateTimestamp();
        }

        public void UpdateDeadline(DateTime? deadline)
        {
            Deadline = deadline;
            UpdateTimestamp();
        }

        public void UpdateEstimatedMinutes(int? minutes)
        {
            SetEstimatedMinutes(minutes);
            UpdateTimestamp();
        }

        public void UpdateStatus(DevTaskStatus newStatus)
        {
            if (Status == DevTaskStatus.Completed && newStatus == DevTaskStatus.Completed)
                return;

            var oldStatus = Status;
            Status = newStatus;

            if (newStatus == DevTaskStatus.Completed)
            {
                CompletedAt = DateTime.UtcNow;
            }
            else if (oldStatus == DevTaskStatus.Completed)
            {
                CompletedAt = null;
            }

            UpdateTimestamp();
        }

        private void SetTitle(string title)
        {
            var normalizedTitle = title?.Trim();

            if (string.IsNullOrWhiteSpace(normalizedTitle))
                throw new ArgumentException("Task title cannot be empty or whitespace.", nameof(title));

            if (normalizedTitle.Length > 200)
                throw new ArgumentException("Task title cannot exceed 200 characters.", nameof(title));

            Title = normalizedTitle;
        }

        private void SetDescription(string? description)
        {
            Description = string.IsNullOrWhiteSpace(description) 
                ? null 
                : description.Trim();
        }

        private void SetEstimatedMinutes(int? minutes)
        {
            if (minutes.HasValue)
            {
                if (minutes.Value <= 0)
                    throw new ArgumentException("Estimated minutes must be greater than 0.", nameof(minutes));

                if (minutes.Value > 10000)
                    throw new ArgumentException("Estimated minutes cannot exceed 10000.", nameof(minutes));
            }

            EstimatedMinutes = minutes;
        }

        private void UpdateTimestamp()
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public enum DevTaskStatus
    {
        Todo,
        InProgress,
        Blocked,
        Completed,
        Cancelled
    }

    public enum TaskPriority
    {
        Low,
        Medium,
        High,
        Critical
    }
}