namespace DevOS.Application.TimeEntries.Commands.UpdateTimeEntry
{
    public record UpdateTimeEntryCommand(
        Guid Id,
        Guid ProjectId,
        DateTime StartedAt,
        DateTime? EndedAt = null,
        string? Description = null,
        Guid? TaskId = null
    );
}