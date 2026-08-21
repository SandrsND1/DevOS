using DevOS.Domain.Entities;

namespace DevOS.Application.Tasks.CreateTask
{
    public class CreateTaskValidator
    {
        public List<string> Validate(CreateTaskCommand command)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(command.Title))
                errors.Add("Task title cannot be empty or whitespace.");

            if (command.Title?.Length > 200)
                errors.Add("Task title cannot exceed 200 characters.");

            if (command.Description?.Length > 2000)
                errors.Add("Task description cannot exceed 2000 characters.");

            if (!Enum.IsDefined(typeof(TaskPriority), command.Priority))
                errors.Add("Priority must be a valid task priority.");

            if (command.EstimatedMinutes.HasValue && command.EstimatedMinutes.Value <= 0)
                errors.Add("Estimated minutes must be greater than 0.");

            if (command.Deadline.HasValue && command.Deadline.Value < DateTime.UtcNow)
                errors.Add("Deadline cannot be in the past.");

            return errors;
        }
    }
}