namespace DevOS.Application.TimeEntries.Queries.GetTimeEntryById
{
    public class GetTimeEntryByIdQuery
    {
        public Guid EntryId { get; init; }
        public Guid ProjectId { get; init; }
    }
}
