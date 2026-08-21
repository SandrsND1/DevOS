using DevOS.Application.Exceptions;
using DevOS.Application.TimeEntries;
using DevOS.Application.TimeEntries.Queries.GetTimeEntryById;
using DevOS.Domain.Entities;
using Moq;

namespace DevOS.Application.Tests.TimeEntries.GetTimeEntryById
{
    public class GetTimeEntryByIdHandlerTests
    {
        [Fact]
        public async Task Handle_ExistingTimeEntry_ReturnsResponse()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var startedAt = DateTime.UtcNow.AddHours(-2);
            var endedAt = DateTime.UtcNow;

            var timeEntry = new TimeEntry(projectId, startedAt, endedAt, "Coding backend");

            var mockRepo = new Mock<ITimeEntryRepository>();
            mockRepo
                .Setup(r => r.GetByIdAsync(timeEntry.Id, projectId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(timeEntry);

            var handler = new GetTimeEntryByIdHandler(mockRepo.Object);

            var query = new GetTimeEntryByIdQuery
            {
                ProjectId = projectId,
                EntryId = timeEntry.Id
            };

            // Act
            var response = await handler.HandleAsync(query);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(timeEntry.Id, response.Id);
            Assert.Equal(projectId, response.ProjectId);
            Assert.Equal(timeEntry.StartedAt, response.StartedAt);
            Assert.Equal(timeEntry.EndedAt, response.EndedAt);
            Assert.Equal("Coding backend", response.Description);

            mockRepo.Verify(
                r => r.GetByIdAsync(timeEntry.Id, projectId, It.IsAny<CancellationToken>()),
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

            var handler = new GetTimeEntryByIdHandler(mockRepo.Object);

            var query = new GetTimeEntryByIdQuery
            {
                ProjectId = projectId,
                EntryId = entryId
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<TimeEntryNotFoundException>(
                () => handler.HandleAsync(query));

            Assert.Equal(entryId, ex.EntryId);
        }
    }
}
