using DevOS.Application.Tasks.CreateTask;
using DevOS.Application.Tasks.GetTasks;
using DevOS.Application.Tasks.UpdateTask;
using DevOS.Domain.Entities;

namespace DevOS.Application.Tests.Tasks.Validation
{
    public class TaskValidatorsTests
    {
        private readonly CreateTaskValidator _createValidator = new();
        private readonly UpdateTaskValidator _updateValidator = new();
        private readonly GetTasksValidator _getTasksValidator = new();

        [Fact]
        public void CreateTaskValidator_ValidCommand_ReturnsNoErrors()
        {
            var command = new CreateTaskCommand
            {
                ProjectId = Guid.NewGuid(),
                Title = "Valid Title",
                Description = "Valid Description",
                Priority = TaskPriority.High,
                EstimatedMinutes = 60,
                Deadline = DateTime.UtcNow.AddDays(5)
            };

            var errors = _createValidator.Validate(command);

            Assert.Empty(errors);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void CreateTaskValidator_EmptyTitle_ReturnsError(string? title)
        {
            var command = new CreateTaskCommand
            {
                Title = title!,
                Priority = TaskPriority.Medium
            };

            var errors = _createValidator.Validate(command);

            Assert.Contains(errors, e => e.Contains("title cannot be empty"));
        }

        [Fact]
        public void CreateTaskValidator_TitleExceedsMaxLength_ReturnsError()
        {
            var command = new CreateTaskCommand
            {
                Title = new string('a', 201),
                Priority = TaskPriority.Medium
            };

            var errors = _createValidator.Validate(command);

            Assert.Contains(errors, e => e.Contains("cannot exceed 200 characters"));
        }

        [Fact]
        public void CreateTaskValidator_DescriptionExceedsMaxLength_ReturnsError()
        {
            var command = new CreateTaskCommand
            {
                Title = "Valid Title",
                Description = new string('d', 2001),
                Priority = TaskPriority.Medium
            };

            var errors = _createValidator.Validate(command);

            Assert.Contains(errors, e => e.Contains("cannot exceed 2000 characters"));
        }

        [Fact]
        public void CreateTaskValidator_InvalidEstimatedMinutes_ReturnsError()
        {
            var command = new CreateTaskCommand
            {
                Title = "Valid Title",
                Priority = TaskPriority.Medium,
                EstimatedMinutes = 0
            };

            var errors = _createValidator.Validate(command);

            Assert.Contains(errors, e => e.Contains("Estimated minutes must be greater than 0"));
        }

        [Fact]
        public void CreateTaskValidator_PastDeadline_ReturnsError()
        {
            var command = new CreateTaskCommand
            {
                Title = "Valid Title",
                Priority = TaskPriority.Medium,
                Deadline = DateTime.UtcNow.AddDays(-1)
            };

            var errors = _createValidator.Validate(command);

            Assert.Contains(errors, e => e.Contains("Deadline cannot be in the past"));
        }

        [Fact]
        public void UpdateTaskValidator_ValidCommand_ReturnsNoErrors()
        {
            var command = new UpdateTaskCommand
            {
                TaskId = Guid.NewGuid(),
                ProjectId = Guid.NewGuid(),
                Title = "Valid Updated Title",
                Description = "Valid Updated Description",
                Status = DevTaskStatus.InProgress,
                Priority = TaskPriority.High,
                EstimatedMinutes = 120,
                Deadline = DateTime.UtcNow.AddDays(10)
            };

            var errors = _updateValidator.Validate(command);

            Assert.Empty(errors);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void UpdateTaskValidator_EmptyTitle_ReturnsError(string? title)
        {
            var command = new UpdateTaskCommand
            {
                Title = title!,
                Status = DevTaskStatus.Todo,
                Priority = TaskPriority.Medium
            };

            var errors = _updateValidator.Validate(command);

            Assert.Contains(errors, e => e.Contains("title cannot be empty"));
        }

        [Fact]
        public void GetTasksValidator_ValidQuery_ReturnsNoErrors()
        {
            var query = new GetTasksQuery
            {
                ProjectId = Guid.NewGuid(),
                Page = 1,
                PageSize = 20,
                Status = "InProgress",
                Priority = "High",
                Search = "Feature",
                SortBy = "Title",
                SortDirection = "asc"
            };

            var errors = _getTasksValidator.Validate(query);

            Assert.Empty(errors);
        }

        [Fact]
        public void GetTasksValidator_InvalidPagination_ReturnsErrors()
        {
            var query = new GetTasksQuery
            {
                ProjectId = Guid.NewGuid(),
                Page = 0,
                PageSize = 101
            };

            var errors = _getTasksValidator.Validate(query);

            Assert.Contains(errors, e => e.Contains("Page must be greater than or equal to 1"));
            Assert.Contains(errors, e => e.Contains("PageSize cannot exceed 100"));
        }

        [Fact]
        public void GetTasksValidator_InvalidStatusAndPriority_ReturnsErrors()
        {
            var query = new GetTasksQuery
            {
                ProjectId = Guid.NewGuid(),
                Status = "InvalidStatus",
                Priority = "InvalidPriority"
            };

            var errors = _getTasksValidator.Validate(query);

            Assert.Contains(errors, e => e.Contains("Status must be one of"));
            Assert.Contains(errors, e => e.Contains("Priority must be one of"));
        }

        [Fact]
        public void GetTasksValidator_InvalidSorting_ReturnsErrors()
        {
            var query = new GetTasksQuery
            {
                ProjectId = Guid.NewGuid(),
                SortBy = "NonExistentField",
                SortDirection = "sideways"
            };

            var errors = _getTasksValidator.Validate(query);

            Assert.Contains(errors, e => e.Contains("SortBy must be one of"));
            Assert.Contains(errors, e => e.Contains("SortDirection must be 'asc' or 'desc'"));
        }
    }
}
