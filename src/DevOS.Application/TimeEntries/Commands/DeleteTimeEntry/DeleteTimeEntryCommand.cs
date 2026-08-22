namespace DevOS.Application.TimeEntries.Commands.DeleteTimeEntry
{
    public record DeleteTimeEntryCommand(
        Guid Id,
        Guid ProjectId
    );
}