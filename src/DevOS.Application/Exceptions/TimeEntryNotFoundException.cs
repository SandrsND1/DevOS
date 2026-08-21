namespace DevOS.Application.Exceptions
{
    public class TimeEntryNotFoundException : Exception
    {
        public Guid EntryId { get; }

        public TimeEntryNotFoundException(Guid entryId)
            : base($"Time entry with ID '{entryId}' was not found.")
        {
            EntryId = entryId;
        }
    }
}
