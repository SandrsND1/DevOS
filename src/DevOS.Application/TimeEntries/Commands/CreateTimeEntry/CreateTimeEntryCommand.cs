namespace DevOS.Application.TimeEntries.Commands.CreateTimeEntry
{
    public class CreateTimeEntryCommand
    {
        public Guid ProjectId { get; init; }
        public Guid? TaskId { get; init; }
        public DateTime StartedAt { get; init; }
        public DateTime EndedAt { get; init; }
        public string? Description { get; init; }
    }
}
