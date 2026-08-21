namespace DevOS.Application.TimeEntries.Queries.GetTimeEntries
{
    public class GetTimeEntriesQuery
    {
        public Guid ProjectId { get; init; }
        public Guid? TaskId { get; init; }
        public DateTime? From { get; init; }
        public DateTime? To { get; init; }
    }
}
