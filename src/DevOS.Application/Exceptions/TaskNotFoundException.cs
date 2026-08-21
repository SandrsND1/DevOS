namespace DevOS.Application.Exceptions
{
    public class TaskNotFoundException : Exception
    {
        public Guid TaskId { get; }

        public TaskNotFoundException(Guid taskId)
            : base($"Task with ID '{taskId}' was not found.")
        {
            TaskId = taskId;
        }
    }
}