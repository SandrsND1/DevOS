using DevOS.Application.Abstractions.Repositories;
using DevOS.Application.Exceptions;
using DevOS.Application.Tasks;
using DevOS.Application.Tasks.CreateTask;
using DevOS.Application.Validation;
using DevOS.Domain.Entities;
using Moq;

namespace DevOS.Application.Tests.Tasks.CreateTask
{
    public class CreateTaskHandlerTests
    {
        [Fact]
        public async Task Handle_ValidCommand_CreatesTaskAndReturnsResponse()
        {
            // Arrange
            var project = new Project("Test Project", ProjectPriority.High);
            var mockProjectRepo = new Mock<IProjectRepository>();
            mockProjectRepo
                .Setup(r => r.GetByIdAsync(project.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(project);

            var mockTaskRepo = new Mock<ITaskRepository>();
            mockTaskRepo
                .Setup(r => r.AddAsync(It.IsAny<DevTask>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var validator = new CreateTaskValidator();
            var handler = new CreateTaskHandler(mockTaskRepo.Object, mockProjectRepo.Object, validator);

            var command = new CreateTaskCommand
            {
                ProjectId = project.Id,
                Title = "Implement Feature X",
                Description = "Detailed description",
                Priority = TaskPriority.High,
                EstimatedMinutes = 120,
                Deadline = DateTime.UtcNow.AddDays(7)
            };

            // Act
            var response = await handler.HandleAsync(command);

            // Assert
            Assert.NotNull(response);
            Assert.NotEqual(Guid.Empty, response.Id);
            Assert.Equal(project.Id, response.ProjectId);
            Assert.Equal(command.Title, response.Title);
            Assert.Equal(command.Description, response.Description);
            Assert.Equal(DevTaskStatus.Todo, response.Status);
            Assert.Equal(command.Priority, response.Priority);
            Assert.Equal(command.EstimatedMinutes, response.EstimatedMinutes);
            Assert.Equal(command.Deadline, response.Deadline);

            mockProjectRepo.Verify(
                r => r.GetByIdAsync(project.Id, It.IsAny<CancellationToken>()),
                Times.Once);

            mockTaskRepo.Verify(
                r => r.AddAsync(
                    It.Is<DevTask>(t => t.ProjectId == project.Id && t.Title == command.Title),
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
            var validator = new CreateTaskValidator();
            var handler = new CreateTaskHandler(mockTaskRepo.Object, mockProjectRepo.Object, validator);

            var command = new CreateTaskCommand
            {
                ProjectId = projectId,
                Title = "Some Task",
                Priority = TaskPriority.Medium
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ProjectNotFoundException>(
                () => handler.HandleAsync(command));

            Assert.Equal(projectId, ex.ProjectId);

            mockTaskRepo.Verify(
                r => r.AddAsync(It.IsAny<DevTask>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_InvalidCommand_ThrowsValidationException()
        {
            // Arrange
            var mockProjectRepo = new Mock<IProjectRepository>();
            var mockTaskRepo = new Mock<ITaskRepository>();
            var validator = new CreateTaskValidator();
            var handler = new CreateTaskHandler(mockTaskRepo.Object, mockProjectRepo.Object, validator);

            var command = new CreateTaskCommand
            {
                ProjectId = Guid.NewGuid(),
                Title = "   ", // Empty title
                Priority = TaskPriority.Medium
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ValidationException>(
                () => handler.HandleAsync(command));

            Assert.NotEmpty(ex.Errors);

            mockProjectRepo.Verify(
                r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
                Times.Never);

            mockTaskRepo.Verify(
                r => r.AddAsync(It.IsAny<DevTask>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
