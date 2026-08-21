using DevOS.Application.Exceptions;
using DevOS.Application.Tasks;
using DevOS.Application.Tasks.UpdateTask;
using DevOS.Application.Validation;
using DevOS.Domain.Entities;
using Moq;

namespace DevOS.Application.Tests.Tasks.UpdateTask
{
    public class UpdateTaskHandlerTests
    {
        [Fact]
        public async Task Handle_ValidCommand_UpdatesTaskAndReturnsResponse()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var existingTask = new DevTask(
                projectId,
                "Old Title",
                TaskPriority.Low,
                "Old Description",
                30);

            var mockTaskRepo = new Mock<ITaskRepository>();
            mockTaskRepo
                .Setup(r => r.GetByIdAsync(existingTask.Id, projectId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingTask);

            mockTaskRepo
                .Setup(r => r.UpdateAsync(It.IsAny<DevTask>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var validator = new UpdateTaskValidator();
            var handler = new UpdateTaskHandler(mockTaskRepo.Object, validator);

            var command = new UpdateTaskCommand
            {
                TaskId = existingTask.Id,
                ProjectId = projectId,
                Title = "Updated Title",
                Description = "Updated Description",
                Status = DevTaskStatus.InProgress,
                Priority = TaskPriority.High,
                EstimatedMinutes = 60,
                Deadline = DateTime.UtcNow.AddDays(10)
            };

            // Act
            var response = await handler.HandleAsync(command);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(existingTask.Id, response.Id);
            Assert.Equal(projectId, response.ProjectId);
            Assert.Equal(command.Title, response.Title);
            Assert.Equal(command.Description, response.Description);
            Assert.Equal(command.Status, response.Status);
            Assert.Equal(command.Priority, response.Priority);
            Assert.Equal(command.EstimatedMinutes, response.EstimatedMinutes);
            Assert.Equal(command.Deadline, response.Deadline);

            mockTaskRepo.Verify(
                r => r.UpdateAsync(
                    It.Is<DevTask>(t =>
                        t.Id == existingTask.Id &&
                        t.Title == command.Title &&
                        t.Status == command.Status &&
                        t.Priority == command.Priority),
                    It.IsAny<CancellationToken>()),
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

            var validator = new UpdateTaskValidator();
            var handler = new UpdateTaskHandler(mockTaskRepo.Object, validator);

            var command = new UpdateTaskCommand
            {
                TaskId = taskId,
                ProjectId = projectId,
                Title = "Valid Title",
                Status = DevTaskStatus.Todo,
                Priority = TaskPriority.Medium
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<TaskNotFoundException>(
                () => handler.HandleAsync(command));

            Assert.Equal(taskId, ex.TaskId);

            mockTaskRepo.Verify(
                r => r.UpdateAsync(It.IsAny<DevTask>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_InvalidCommand_ThrowsValidationException()
        {
            // Arrange
            var mockTaskRepo = new Mock<ITaskRepository>();
            var validator = new UpdateTaskValidator();
            var handler = new UpdateTaskHandler(mockTaskRepo.Object, validator);

            var command = new UpdateTaskCommand
            {
                TaskId = Guid.NewGuid(),
                ProjectId = Guid.NewGuid(),
                Title = "   ", // Empty title
                Status = DevTaskStatus.Todo,
                Priority = TaskPriority.Medium
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ValidationException>(
                () => handler.HandleAsync(command));

            Assert.NotEmpty(ex.Errors);

            mockTaskRepo.Verify(
                r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
                Times.Never);

            mockTaskRepo.Verify(
                r => r.UpdateAsync(It.IsAny<DevTask>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
