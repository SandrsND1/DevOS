namespace DevOS.Application.TimeEntries.DTOs
{
    public class TimeEntryDto
    {
        public Guid Id { get; init; }
        public Guid ProjectId { get; init; }
        public Guid? TaskId { get; init; }
        public DateTime StartedAt { get; init; }
        public DateTime EndedAt { get; init; }
        public int DurationMinutes { get; init; }
        public string? Description { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
    }
}
