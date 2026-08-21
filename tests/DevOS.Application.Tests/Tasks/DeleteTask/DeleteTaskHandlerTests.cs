using DevOS.Application.Exceptions;
using DevOS.Application.Tasks;
using DevOS.Application.Tasks.DeleteTask;
using DevOS.Domain.Entities;
using Moq;

namespace DevOS.Application.Tests.Tasks.DeleteTask
{
    public class DeleteTaskHandlerTests
    {
        [Fact]
        public async Task Handle_ExistingTask_DeletesTask()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var task = new DevTask(projectId, "Task to Delete", TaskPriority.Medium);

            var mockTaskRepo = new Mock<ITaskRepository>();
            mockTaskRepo
                .Setup(r => r.GetByIdAsync(task.Id, projectId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(task);

            mockTaskRepo
                .Setup(r => r.DeleteAsync(task, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var handler = new DeleteTaskHandler(mockTaskRepo.Object);

            var command = new DeleteTaskCommand
            {
                ProjectId = projectId,
                TaskId = task.Id
            };

            // Act
            await handler.HandleAsync(command);

            // Assert
            mockTaskRepo.Verify(
                r => r.GetByIdAsync(task.Id, projectId, It.IsAny<CancellationToken>()),
                Times.Once);

            mockTaskRepo.Verify(
                r => r.DeleteAsync(task, It.IsAny<CancellationToken>()),
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

            var handler = new DeleteTaskHandler(mockTaskRepo.Object);

            var command = new DeleteTaskCommand
            {
                ProjectId = projectId,
                TaskId = taskId
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<TaskNotFoundException>(
                () => handler.HandleAsync(command));

            Assert.Equal(taskId, ex.TaskId);

            mockTaskRepo.Verify(
                r => r.GetByIdAsync(taskId, projectId, It.IsAny<CancellationToken>()),
                Times.Once);

            mockTaskRepo.Verify(
                r => r.DeleteAsync(It.IsAny<DevTask>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
