namespace DevOS.Domain.Entities
{
    public class Project
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string? Description { get; private set; }
        public ProjectStatus Status { get; private set; }
        public ProjectPriority Priority { get; private set; }
        public DateTime? Deadline { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        // Parameterless constructor for EF Core
        private Project() { }

        public Project(
            string name,
            ProjectPriority priority,
            string? description = null,
            DateTime? deadline = null)
        {
            Id = Guid.NewGuid();
            SetName(name);
            SetDescription(description);
            Priority = priority;
            Status = ProjectStatus.Planning;
            Deadline = deadline;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateName(string name)
        {
            SetName(name);
            UpdateTimestamp();
        }

        public void UpdateDescription(string? description)
        {
            SetDescription(description);
            UpdateTimestamp();
        }

        public void UpdateStatus(ProjectStatus status)
        {
            Status = status;
            UpdateTimestamp();
        }

        public void UpdatePriority(ProjectPriority priority)
        {
            Priority = priority;
            UpdateTimestamp();
        }

        public void UpdateDeadline(DateTime? deadline)
        {
            Deadline = deadline;
            UpdateTimestamp();
        }

        private void SetName(string name)
        {
            var normalizedName = name?.Trim();

            if (string.IsNullOrWhiteSpace(normalizedName))
                throw new ArgumentException("Project name cannot be empty or whitespace.", nameof(name));

            if (normalizedName.Length > 100)
                throw new ArgumentException("Project name cannot exceed 100 characters.", nameof(name));

            Name = normalizedName;
        }

        private void SetDescription(string? description)
        {
            Description = string.IsNullOrWhiteSpace(description) 
                ? null 
                : description.Trim();
        }

        private void UpdateTimestamp()
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public enum ProjectStatus
    {
        Planning,
        Active,
        Paused,
        Completed,
        Archived
    }

    public enum ProjectPriority
    {
        Low,
        Medium,
        High,
        Critical
    }
}