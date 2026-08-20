namespace DevOS.Domain.Entities
{
    // Represents a user project in the DevOS system
    public class Project
    {
        // Unique identifier for the project, generated upon creation
        public Guid Id { get; private set; }

        // Project name, must be non-empty and max 100 characters
        public string Name { get; private set; }

        // Optional project description, null if not provided
        public string? Description { get; private set; }

        // Current lifecycle state of the project
        public ProjectStatus Status { get; private set; }

        // Priority level of the project
        public ProjectPriority Priority { get; private set; }

        // Optional deadline, null if no deadline set
        public DateTime? Deadline { get; private set; }

        // UTC timestamp when project was created
        public DateTime CreatedAt { get; private set; }

        // UTC timestamp of last modification
        public DateTime UpdatedAt { get; private set; }

        // Parameterless constructor for EF Core
        private Project() { }

        // Creates new project with Planning status and Medium priority
        public Project(string name, string? description = null, DateTime? deadline = null)
        {
            Id = Guid.NewGuid();
            SetName(name);
            SetDescription(description);
            Status = ProjectStatus.Planning;
            Priority = ProjectPriority.Medium;
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

        // Normalizes and validates name: trim → check empty → check length → assign
        private void SetName(string name)
        {
            var normalizedName = name?.Trim();

            if (string.IsNullOrWhiteSpace(normalizedName))
                throw new ArgumentException("Project name cannot be empty or whitespace.", nameof(name));

            if (normalizedName.Length > 100)
                throw new ArgumentException("Project name cannot exceed 100 characters.", nameof(name));

            Name = normalizedName;
        }

        // Normalizes description: null or whitespace becomes null, otherwise trimmed
        private void SetDescription(string? description)
        {
            Description = string.IsNullOrWhiteSpace(description) 
                ? null 
                : description.Trim();
        }

        // Updates the UpdatedAt timestamp to current UTC time
        private void UpdateTimestamp()
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }

    // Lifecycle stages of a project
    public enum ProjectStatus
    {
        Planning,
        Active,
        Paused,
        Completed,
        Archived
    }

    // Priority levels for projects
    public enum ProjectPriority
    {
        Low,
        Medium,
        High,
        Critical
    }
}