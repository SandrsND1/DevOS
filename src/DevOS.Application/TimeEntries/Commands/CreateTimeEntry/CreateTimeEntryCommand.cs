namespace DevOS.Application.TimeEntries.Commands.CreateTimeEntry
{
    public record CreateTimeEntryCommand(
        Guid ProjectId,
        DateTime StartedAt,
        DateTime? EndedAt = null,
        string? Description = null,
        Guid? TaskId = null
    );
}