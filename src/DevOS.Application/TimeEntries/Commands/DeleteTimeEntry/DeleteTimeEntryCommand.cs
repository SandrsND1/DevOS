namespace DevOS.Application.TimeEntries.Commands.DeleteTimeEntry
{
    public class DeleteTimeEntryCommand
    {
        public Guid EntryId { get; init; }
        public Guid ProjectId { get; init; }
    }
}
