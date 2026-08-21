namespace DevOS.Application.TimeEntries.Commands.CreateTimeEntry
{
    public class CreateTimeEntryValidator
    {
        public List<string> Validate(CreateTimeEntryCommand command)
        {
            var errors = new List<string>();

            if (command.StartedAt >= command.EndedAt)
                errors.Add("StartedAt must be earlier than EndedAt.");

            if (command.Description?.Length > 2000)
                errors.Add("Description cannot exceed 2000 characters.");

            return errors;
        }
    }
}
