using DevOS.Application.Exceptions;
using DevOS.Application.Abstractions.Repositories;
using DevOS.Application.Tasks;
using DevOS.Application.Validation;
using DevOS.Domain.Entities;

namespace DevOS.Application.Tasks.GetTasks
{
    public class GetTasksHandler
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly GetTasksValidator _validator;

        public GetTasksHandler(
            ITaskRepository taskRepository,
            IProjectRepository projectRepository,
            GetTasksValidator validator)
        {
            _taskRepository = taskRepository;
            _projectRepository = projectRepository;
            _validator = validator;
        }

        public async Task<GetTasksResponse> HandleAsync(
            GetTasksQuery query,
            CancellationToken cancellationToken = default)
        {
            var validationErrors = _validator.Validate(query);

            if (validationErrors.Count > 0)
                throw new ValidationException(validationErrors);

            var project = await _projectRepository.GetByIdAsync(query.ProjectId, cancellationToken);

            if (project is null)
                throw new ProjectNotFoundException(query.ProjectId);

            var status = ParseStatus(query.Status);
            var priority = ParsePriority(query.Priority);

            var totalCount = await _taskRepository.GetTotalCountAsync(
                query.ProjectId,
                status,
                priority,
                query.Search,
                cancellationToken);

            var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);

            var tasks = await _taskRepository.GetPagedAsync(
                query.ProjectId,
                query.Page,
                query.PageSize,
                status,
                priority,
                query.Search,
                query.SortBy,
                query.SortDirection,
                cancellationToken);

            return new GetTasksResponse
            {
                Items = tasks.Select(t => new TaskItem
                {
                    Id = t.Id,
                    ProjectId = t.ProjectId,
                    Title = t.Title,
                    Description = t.Description,
                    Status = t.Status,
                    Priority = t.Priority,
                    EstimatedMinutes = t.EstimatedMinutes,
                    Deadline = t.Deadline,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt
                }).ToList(),
                Page = query.Page,
                PageSize = query.PageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
            };
        }

        private static DevTaskStatus? ParseStatus(string? status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return null;

            return Enum.Parse<DevTaskStatus>(status, true);
        }

        private static TaskPriority? ParsePriority(string? priority)
        {
            if (string.IsNullOrWhiteSpace(priority))
                return null;

            return Enum.Parse<TaskPriority>(priority, true);
        }
    }
}
