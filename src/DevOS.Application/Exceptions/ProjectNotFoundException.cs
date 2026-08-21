namespace DevOS.Application.Exceptions
{
    public class ProjectNotFoundException : Exception
    {
        public Guid ProjectId { get; }

        public ProjectNotFoundException(Guid projectId)
            : base($"Project with ID '{projectId}' was not found.")
        {
            ProjectId = projectId;
        }
    }
}