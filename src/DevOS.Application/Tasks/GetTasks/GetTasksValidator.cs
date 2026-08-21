using DevOS.Domain.Entities;

namespace DevOS.Application.Tasks.GetTasks
{
    public class GetTasksValidator
    {
        private static readonly HashSet<string> AllowedSortFields = new()
        {
            "CreatedAt",
            "UpdatedAt",
            "Title",
            "Deadline",
            "Priority",
            "Status",
            "EstimatedMinutes"
        };

        private static readonly HashSet<string> AllowedSortDirections = new()
        {
            "asc",
            "desc"
        };

        public List<string> Validate(GetTasksQuery query)
        {
            var errors = new List<string>();

            if (query.Page < 1)
                errors.Add("Page must be greater than or equal to 1.");

            if (query.PageSize < 1)
                errors.Add("PageSize must be greater than or equal to 1.");

            if (query.PageSize > 100)
                errors.Add("PageSize cannot exceed 100.");

            if (!string.IsNullOrWhiteSpace(query.Status) && 
                !Enum.TryParse<DevTaskStatus>(query.Status, true, out _))
                errors.Add($"Status must be one of: {string.Join(", ", Enum.GetNames<DevTaskStatus>())}.");

            if (!string.IsNullOrWhiteSpace(query.Priority) && 
                !Enum.TryParse<TaskPriority>(query.Priority, true, out _))
                errors.Add($"Priority must be one of: {string.Join(", ", Enum.GetNames<TaskPriority>())}.");

            if (!string.IsNullOrWhiteSpace(query.Search) && query.Search.Length > 100)
                errors.Add("Search cannot exceed 100 characters.");

            if (!AllowedSortFields.Contains(query.SortBy))
                errors.Add($"SortBy must be one of: {string.Join(", ", AllowedSortFields)}.");

            if (!AllowedSortDirections.Contains(query.SortDirection.ToLower()))
                errors.Add("SortDirection must be 'asc' or 'desc'.");

            return errors;
        }
    }
}