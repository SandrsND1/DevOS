namespace DevOS.Application.TimeEntries.Queries.GetTimeEntries
{
    public class GetTimeEntriesValidator
    {
        public List<string> Validate(GetTimeEntriesQuery query)
        {
            var errors = new List<string>();

            if (query.From.HasValue && query.To.HasValue && query.From.Value >= query.To.Value)
                errors.Add("'From' date must be earlier than 'To' date.");

            return errors;
        }
    }
}
