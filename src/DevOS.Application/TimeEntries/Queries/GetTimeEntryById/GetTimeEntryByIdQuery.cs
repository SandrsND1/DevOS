namespace DevOS.Application.TimeEntries.Queries.GetTimeEntryById
{
    public class GetTimeEntryByIdQuery
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
    }
}