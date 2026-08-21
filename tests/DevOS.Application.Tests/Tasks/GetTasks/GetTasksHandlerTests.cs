using DevOS.Application.Abstractions.Repositories;
using DevOS.Application.Exceptions;
using DevOS.Application.Tasks;
using DevOS.Application.Tasks.GetTasks;
using DevOS.Application.Validation;
using DevOS.Domain.Entities;
using Moq;

namespace DevOS.Application.Tests.Tasks.GetTasks
{
    public class GetTasksHandlerTests
    {
        [Fact]
        public async Task Handle_ValidQuery_ReturnsTasksWithPagination()
        {
            // Arrange
            var project = new Project("Test Project", ProjectPriority.High);
            var mockProjectRepo = new Mock<IProjectRepository>();
            mockProjectRepo
                .Setup(r => r.GetByIdAsync(project.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(project);

            var task1 = new DevTask(project.Id, "Task 1", TaskPriority.Medium, "Desc 1", 60);
            var task2 = new DevTask(project.Id, "Task 2", TaskPriority.High, "Desc 2", 120);

            var mockTaskRepo = new Mock<ITaskRepository>();
            mockTaskRepo
                .Setup(r => r.GetTotalCountAsync(
                    project.Id,
                    null,
                    null,
                    null,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(15);

            mockTaskRepo
                .Setup(r => r.GetPagedAsync(
                    project.Id,
                    1,
                    10,
                    null,
                    null,
                    null,
                    "CreatedAt",
                    "desc",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<DevTask> { task1, task2 });

            var validator = new GetTasksValidator();
            var handler = new GetTasksHandler(mockTaskRepo.Object, mockProjectRepo.Object, validator);

            var query = new GetTasksQuery
            {
                ProjectId = project.Id,
                Page = 1,
                PageSize = 10
            };

            // Act
            var response = await handler.HandleAsync(query);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(1, response.Page);
            Assert.Equal(10, response.PageSize);
            Assert.Equal(15, response.TotalCount);
            Assert.Equal(2, response.TotalPages);
            Assert.Equal(2, response.Items.Count);

            Assert.Equal(task1.Id, response.Items[0].Id);
            Assert.Equal(task1.Title, response.Items[0].Title);
            Assert.Equal(task2.Id, response.Items[1].Id);
            Assert.Equal(task2.Title, response.Items[1].Title);

            mockProjectRepo.Verify(
                r => r.GetByIdAsync(project.Id, It.IsAny<CancellationToken>()),
                Times.Once);

            mockTaskRepo.Verify(
                r => r.GetTotalCountAsync(
                    project.Id,
                    null,
                    null,
                    null,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            mockTaskRepo.Verify(
                r => r.GetPagedAsync(
                    project.Id,
                    1,
                    10,
                    null,
                    null,
                    null,
                    "CreatedAt",
                    "desc",
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_WithFiltersAndSorting_ParsesAndPassesToRepository()
        {
            // Arrange
            var project = new Project("Test Project", ProjectPriority.High);
            var mockProjectRepo = new Mock<IProjectRepository>();
            mockProjectRepo
                .Setup(r => r.GetByIdAsync(project.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(project);

            var mockTaskRepo = new Mock<ITaskRepository>();
            mockTaskRepo
                .Setup(r => r.GetTotalCountAsync(
                    project.Id,
                    DevTaskStatus.InProgress,
                    TaskPriority.Critical,
                    "Auth",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(3);

            mockTaskRepo
                .Setup(r => r.GetPagedAsync(
                    project.Id,
                    1,
                    10,
                    DevTaskStatus.InProgress,
                    TaskPriority.Critical,
                    "Auth",
                    "Title",
                    "asc",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<DevTask>());

            var validator = new GetTasksValidator();
            var handler = new GetTasksHandler(mockTaskRepo.Object, mockProjectRepo.Object, validator);

            var query = new GetTasksQuery
            {
                ProjectId = project.Id,
                Page = 1,
                PageSize = 10,
                Status = "InProgress",
                Priority = "Critical",
                Search = "Auth",
                SortBy = "Title",
                SortDirection = "asc"
            };

            // Act
            var response = await handler.HandleAsync(query);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(3, response.TotalCount);
            Assert.Equal(1, response.TotalPages);

            mockTaskRepo.Verify(
                r => r.GetTotalCountAsync(
                    project.Id,
                    DevTaskStatus.InProgress,
                    TaskPriority.Critical,
                    "Auth",
                    It.IsAny<CancellationToken>()),
                Times.Once);

            mockTaskRepo.Verify(
                r => r.GetPagedAsync(
                    project.Id,
                    1,
                    10,
                    DevTaskStatus.InProgress,
                    TaskPriority.Critical,
                    "Auth",
                    "Title",
                    "asc",
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ProjectNotFound_ThrowsProjectNotFoundException()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var mockProjectRepo = new Mock<IProjectRepository>();
            mockProjectRepo
                .Setup(r => r.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Project?)null);

            var mockTaskRepo = new Mock<ITaskRepository>();
            var validator = new GetTasksValidator();
            var handler = new GetTasksHandler(mockTaskRepo.Object, mockProjectRepo.Object, validator);

            var query = new GetTasksQuery
            {
                ProjectId = projectId,
                Page = 1,
                PageSize = 10
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ProjectNotFoundException>(
                () => handler.HandleAsync(query));

            Assert.Equal(projectId, ex.ProjectId);

            mockTaskRepo.Verify(
                r => r.GetPagedAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<DevTaskStatus?>(),
                    It.IsAny<TaskPriority?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_InvalidQuery_ThrowsValidationException()
        {
            // Arrange
            var mockProjectRepo = new Mock<IProjectRepository>();
            var mockTaskRepo = new Mock<ITaskRepository>();
            var validator = new GetTasksValidator();
            var handler = new GetTasksHandler(mockTaskRepo.Object, mockProjectRepo.Object, validator);

            var query = new GetTasksQuery
            {
                ProjectId = Guid.NewGuid(),
                Page = 0, // Invalid page
                PageSize = 200 // Exceeds max 100
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ValidationException>(
                () => handler.HandleAsync(query));

            Assert.NotEmpty(ex.Errors);

            mockProjectRepo.Verify(
                r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
