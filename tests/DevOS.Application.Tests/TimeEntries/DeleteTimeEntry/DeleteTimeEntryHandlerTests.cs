using DevOS.Application.Exceptions;
using DevOS.Application.TimeEntries;
using DevOS.Application.TimeEntries.Commands.DeleteTimeEntry;
using DevOS.Domain.Entities;
using Moq;

namespace DevOS.Application.Tests.TimeEntries.DeleteTimeEntry
{
    public class DeleteTimeEntryHandlerTests
    {
        [Fact]
        public async Task Handle_ExistingTimeEntry_DeletesEntry()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var timeEntry = new TimeEntry(projectId, DateTime.UtcNow.AddHours(-1), DateTime.UtcNow, "To Delete");

            var mockRepo = new Mock<ITimeEntryRepository>();
            mockRepo
                .Setup(r => r.GetByIdAsync(timeEntry.Id, projectId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(timeEntry);

            mockRepo
                .Setup(r => r.DeleteAsync(timeEntry, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var handler = new DeleteTimeEntryHandler(mockRepo.Object);

            var command = new DeleteTimeEntryCommand
            {
                ProjectId = projectId,
                EntryId = timeEntry.Id
            };

            // Act
            await handler.HandleAsync(command);

            // Assert
            mockRepo.Verify(
                r => r.DeleteAsync(timeEntry, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_TimeEntryNotFound_ThrowsTimeEntryNotFoundException()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var entryId = Guid.NewGuid();

            var mockRepo = new Mock<ITimeEntryRepository>();
            mockRepo
                .Setup(r => r.GetByIdAsync(entryId, projectId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((TimeEntry?)null);

            var handler = new DeleteTimeEntryHandler(mockRepo.Object);

            var command = new DeleteTimeEntryCommand
            {
                ProjectId = projectId,
                EntryId = entryId
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<TimeEntryNotFoundException>(
                () => handler.HandleAsync(command));

            Assert.Equal(entryId, ex.EntryId);

            mockRepo.Verify(
                r => r.DeleteAsync(It.IsAny<TimeEntry>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
