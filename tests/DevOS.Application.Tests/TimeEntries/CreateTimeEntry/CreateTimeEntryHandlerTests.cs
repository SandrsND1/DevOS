using DevOS.Application.Abstractions.Repositories;
using DevOS.Application.Exceptions;
using DevOS.Application.Tasks;
using DevOS.Application.TimeEntries;
using DevOS.Application.TimeEntries.Commands.CreateTimeEntry;
using DevOS.Application.Validation;
using DevOS.Domain.Entities;
using Moq;

namespace DevOS.Application.Tests.TimeEntries.CreateTimeEntry
{
    public class CreateTimeEntryHandlerTests
    {
        [Fact]
        public async Task Handle_ValidCommandWithTask_CreatesTimeEntryAndReturnsResponse()
        {
            // Arrange
            var project = new Project("Test Project", ProjectPriority.High);
            var task = new DevTask(project.Id, "Test Task", TaskPriority.Medium);

            var mockProjectRepo = new Mock<IProjectRepository>();
            mockProjectRepo
                .Setup(r => r.GetByIdAsync(project.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(project);

            var mockTaskRepo = new Mock<ITaskRepository>();
            mockTaskRepo
                .Setup(r => r.GetByIdAsync(task.Id, project.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(task);

            var mockTimeEntryRepo = new Mock<ITimeEntryRepository>();
            mockTimeEntryRepo
                .Setup(r => r.AddAsync(It.IsAny<TimeEntry>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var validator = new CreateTimeEntryValidator();
            var handler = new CreateTimeEntryHandler(
                mockTimeEntryRepo.Object,
                mockProjectRepo.Object,
                mockTaskRepo.Object,
                validator);

            var startedAt = new DateTime(2026, 8, 21, 10, 0, 0, DateTimeKind.Utc);
            var endedAt = new DateTime(2026, 8, 21, 11, 30, 0, DateTimeKind.Utc);

            var command = new CreateTimeEntryCommand
            {
                ProjectId = project.Id,
                TaskId = task.Id,
                StartedAt = startedAt,
                EndedAt = endedAt,
                Description = "Coding session"
            };

            // Act
            var response = await handler.HandleAsync(command);

            // Assert
            Assert.NotNull(response);
            Assert.NotEqual(Guid.Empty, response.Id);
            Assert.Equal(project.Id, response.ProjectId);
            Assert.Equal(task.Id, response.TaskId);
            Assert.Equal(startedAt, response.StartedAt);
            Assert.Equal(endedAt, response.EndedAt);
            Assert.Equal(90, response.DurationMinutes);
            Assert.Equal("Coding session", response.Description);

            mockTimeEntryRepo.Verify(
                r => r.AddAsync(
                    It.Is<TimeEntry>(t => t.ProjectId == project.Id && t.TaskId == task.Id),
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
            var mockTimeEntryRepo = new Mock<ITimeEntryRepository>();
            var validator = new CreateTimeEntryValidator();

            var handler = new CreateTimeEntryHandler(
                mockTimeEntryRepo.Object,
                mockProjectRepo.Object,
                mockTaskRepo.Object,
                validator);

            var command = new CreateTimeEntryCommand
            {
                ProjectId = projectId,
                StartedAt = DateTime.UtcNow,
                EndedAt = DateTime.UtcNow.AddHours(1)
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ProjectNotFoundException>(
                () => handler.HandleAsync(command));

            Assert.Equal(projectId, ex.ProjectId);
        }

        [Fact]
        public async Task Handle_TaskNotFound_ThrowsTaskNotFoundException()
        {
            // Arrange
            var project = new Project("Test Project", ProjectPriority.High);
            var taskId = Guid.NewGuid();

            var mockProjectRepo = new Mock<IProjectRepository>();
            mockProjectRepo
                .Setup(r => r.GetByIdAsync(project.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(project);

            var mockTaskRepo = new Mock<ITaskRepository>();
            mockTaskRepo
                .Setup(r => r.GetByIdAsync(taskId, project.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((DevTask?)null);

            var mockTimeEntryRepo = new Mock<ITimeEntryRepository>();
            var validator = new CreateTimeEntryValidator();

            var handler = new CreateTimeEntryHandler(
                mockTimeEntryRepo.Object,
                mockProjectRepo.Object,
                mockTaskRepo.Object,
                validator);

            var command = new CreateTimeEntryCommand
            {
                ProjectId = project.Id,
                TaskId = taskId,
                StartedAt = DateTime.UtcNow,
                EndedAt = DateTime.UtcNow.AddHours(1)
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<TaskNotFoundException>(
                () => handler.HandleAsync(command));

            Assert.Equal(taskId, ex.TaskId);
        }

        [Fact]
        public async Task Handle_InvalidTimes_ThrowsValidationException()
        {
            // Arrange
            var mockProjectRepo = new Mock<IProjectRepository>();
            var mockTaskRepo = new Mock<ITaskRepository>();
            var mockTimeEntryRepo = new Mock<ITimeEntryRepository>();
            var validator = new CreateTimeEntryValidator();

            var handler = new CreateTimeEntryHandler(
                mockTimeEntryRepo.Object,
                mockProjectRepo.Object,
                mockTaskRepo.Object,
                validator);

            var now = DateTime.UtcNow;
            var command = new CreateTimeEntryCommand
            {
                ProjectId = Guid.NewGuid(),
                StartedAt = now,
                EndedAt = now.AddHours(-1) // Invalid range
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ValidationException>(
                () => handler.HandleAsync(command));

            Assert.NotEmpty(ex.Errors);
        }
    }
}
