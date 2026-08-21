using DevOS.Application.Exceptions;
using DevOS.Application.Tasks;
using DevOS.Application.Tasks.GetTask;
using DevOS.Domain.Entities;
using Moq;

namespace DevOS.Application.Tests.Tasks.GetTask
{
    public class GetTaskHandlerTests
    {
        [Fact]
        public async Task Handle_ExistingTask_ReturnsResponse()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var task = new DevTask(
                projectId,
                "Test Task Title",
                TaskPriority.Critical,
                "Test Task Description",
                90,
                DateTime.UtcNow.AddDays(5));

            var mockTaskRepo = new Mock<ITaskRepository>();
            mockTaskRepo
                .Setup(r => r.GetByIdAsync(task.Id, projectId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(task);

            var handler = new GetTaskHandler(mockTaskRepo.Object);

            var query = new GetTaskQuery
            {
                ProjectId = projectId,
                TaskId = task.Id
            };

            // Act
            var response = await handler.HandleAsync(query);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(task.Id, response.Id);
            Assert.Equal(projectId, response.ProjectId);
            Assert.Equal(task.Title, response.Title);
            Assert.Equal(task.Description, response.Description);
            Assert.Equal(task.Status, response.Status);
            Assert.Equal(task.Priority, response.Priority);
            Assert.Equal(task.EstimatedMinutes, response.EstimatedMinutes);
            Assert.Equal(task.Deadline, response.Deadline);
            Assert.Equal(task.CreatedAt, response.CreatedAt);
            Assert.Equal(task.UpdatedAt, response.UpdatedAt);

            mockTaskRepo.Verify(
                r => r.GetByIdAsync(task.Id, projectId, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_TaskNotFound_ThrowsTaskNotFoundException()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var taskId = Guid.NewGuid();

            var mockTaskRepo = new Mock<ITaskRepository>();
            mockTaskRepo
                .Setup(r => r.GetByIdAsync(taskId, projectId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((DevTask?)null);

            var handler = new GetTaskHandler(mockTaskRepo.Object);

            var query = new GetTaskQuery
            {
                ProjectId = projectId,
                TaskId = taskId
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<TaskNotFoundException>(
                () => handler.HandleAsync(query));

            Assert.Equal(taskId, ex.TaskId);

            mockTaskRepo.Verify(
                r => r.GetByIdAsync(taskId, projectId, It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
