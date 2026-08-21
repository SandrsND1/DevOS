using DevOS.Application.Exceptions;
using DevOS.Application.Tasks;
using DevOS.Application.TimeEntries;
using DevOS.Application.TimeEntries.Commands.UpdateTimeEntry;
using DevOS.Application.Validation;
using DevOS.Domain.Entities;
using Moq;

namespace DevOS.Application.Tests.TimeEntries.UpdateTimeEntry
{
    public class UpdateTimeEntryHandlerTests
    {
        [Fact]
        public async Task Handle_ValidCommand_UpdatesTimeEntryAndReturnsResponse()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var existingEntry = new TimeEntry(
                projectId,
                DateTime.UtcNow.AddHours(-2),
                DateTime.UtcNow.AddHours(-1),
                "Old Description");

            var mockTimeEntryRepo = new Mock<ITimeEntryRepository>();
            mockTimeEntryRepo
                .Setup(r => r.GetByIdAsync(existingEntry.Id, projectId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingEntry);

            mockTimeEntryRepo
                .Setup(r => r.UpdateAsync(It.IsAny<TimeEntry>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var mockTaskRepo = new Mock<ITaskRepository>();

            var validator = new UpdateTimeEntryValidator();
            var handler = new UpdateTimeEntryHandler(
                mockTimeEntryRepo.Object,
                mockTaskRepo.Object,
                validator);

            var baseTime = new DateTime(2026, 8, 21, 14, 0, 0, DateTimeKind.Utc);
            var newStartedAt = baseTime.AddHours(-3);
            var newEndedAt = baseTime.AddHours(-1);

            var command = new UpdateTimeEntryCommand
            {
                EntryId = existingEntry.Id,
                ProjectId = projectId,
                StartedAt = newStartedAt,
                EndedAt = newEndedAt,
                Description = "Updated Description"
            };

            // Act
            var response = await handler.HandleAsync(command);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(existingEntry.Id, response.Id);
            Assert.Equal(newStartedAt, response.StartedAt);
            Assert.Equal(newEndedAt, response.EndedAt);
            Assert.Equal(120, response.DurationMinutes);
            Assert.Equal("Updated Description", response.Description);

            mockTimeEntryRepo.Verify(
                r => r.UpdateAsync(
                    It.Is<TimeEntry>(t => t.Id == existingEntry.Id && t.Description == "Updated Description"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_TimeEntryNotFound_ThrowsTimeEntryNotFoundException()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var entryId = Guid.NewGuid();

            var mockTimeEntryRepo = new Mock<ITimeEntryRepository>();
            mockTimeEntryRepo
                .Setup(r => r.GetByIdAsync(entryId, projectId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((TimeEntry?)null);

            var mockTaskRepo = new Mock<ITaskRepository>();
            var validator = new UpdateTimeEntryValidator();
            var handler = new UpdateTimeEntryHandler(mockTimeEntryRepo.Object, mockTaskRepo.Object, validator);

            var command = new UpdateTimeEntryCommand
            {
                EntryId = entryId,
                ProjectId = projectId,
                StartedAt = DateTime.UtcNow,
                EndedAt = DateTime.UtcNow.AddHours(1)
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<TimeEntryNotFoundException>(
                () => handler.HandleAsync(command));

            Assert.Equal(entryId, ex.EntryId);
        }
    }
}
