namespace DevOS.Application.TimeEntries.Commands.UpdateTimeEntry
{
    public class UpdateTimeEntryValidator
    {
        public List<string> Validate(UpdateTimeEntryCommand command)
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
