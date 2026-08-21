namespace DevOS.Domain.Entities
{
    public class TimeEntry
    {
        public Guid Id { get; private set; }
        public Guid ProjectId { get; private set; }
        public Guid? TaskId { get; private set; }
        public DateTime StartedAt { get; private set; }
        public DateTime EndedAt { get; private set; }
        public int DurationMinutes { get; private set; }
        public string? Description { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        // Parameterless constructor for EF Core
        private TimeEntry() { }

        public TimeEntry(
            Guid projectId,
            DateTime startedAt,
            DateTime endedAt,
            string? description = null,
            Guid? taskId = null)
        {
            if (projectId == Guid.Empty)
                throw new ArgumentException("Project ID cannot be empty.", nameof(projectId));

            if (taskId.HasValue && taskId.Value == Guid.Empty)
                throw new ArgumentException("Task ID cannot be empty.", nameof(taskId));

            SetTimeRange(startedAt, endedAt);

            Id = Guid.NewGuid();
            ProjectId = projectId;
            TaskId = taskId;
            Description = NormalizeDescription(description);
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateTimeRange(DateTime startedAt, DateTime endedAt)
        {
            SetTimeRange(startedAt, endedAt);
            UpdateTimestamp();
        }

        public void UpdateDescription(string? description)
        {
            Description = NormalizeDescription(description);
            UpdateTimestamp();
        }

        public void UpdateTask(Guid? taskId)
        {
            if (taskId.HasValue && taskId.Value == Guid.Empty)
                throw new ArgumentException("Task ID cannot be empty.", nameof(taskId));

            TaskId = taskId;
            UpdateTimestamp();
        }

        private void SetTimeRange(DateTime startedAt, DateTime endedAt)
        {
            if (startedAt >= endedAt)
                throw new ArgumentException("StartedAt must be earlier than EndedAt.");

            StartedAt = startedAt;
            EndedAt = endedAt;
            DurationMinutes = (int)Math.Ceiling((endedAt - startedAt).TotalMinutes);
        }

        private static string? NormalizeDescription(string? description)
        {
            return string.IsNullOrWhiteSpace(description)
                ? null
                : description.Trim();
        }

        private void UpdateTimestamp()
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }
}