namespace DevOS.Application.Analytics.DTOs
{
    public class ProjectAnalyticsDto
    {
        public required Guid ProjectId { get; init; }
        public required long TotalTimeSpentMinutes { get; init; }
        public required int TotalTasks { get; init; }
        public required int CompletedTasks { get; init; }
        public required double CompletionPercentage { get; init; }
        public required IReadOnlyDictionary<string, int> TasksByStatus { get; init; }
    }
}