namespace DevOS.Application.Tasks.DeleteTask
{
    public class DeleteTaskCommand
    {
        public Guid ProjectId { get; init; }
        public Guid TaskId { get; init; }
    }
}