using DevOS.Application.Abstractions.Repositories;
using DevOS.Application.Abstractions.Services;
using DevOS.Application.TimeEntries;
using DevOS.Application.TimeEntries.Queries.GetTimeEntryById;
using DevOS.Domain.Entities;
using Moq;

namespace DevOS.Application.Tests.TimeEntries.GetTimeEntryById
{
    public class GetTimeEntryByIdHandlerTests
    {
        private readonly Mock<ITimeEntryRepository> _timeEntryRepoMock;
        private readonly Mock<IProjectRepository> _projectRepoMock;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly GetTimeEntryByIdHandler _handler;

        public GetTimeEntryByIdHandlerTests()
        {
            _timeEntryRepoMock = new Mock<ITimeEntryRepository>();
            _projectRepoMock = new Mock<IProjectRepository>();
            _currentUserServiceMock = new Mock<ICurrentUserService>();
            _handler = new GetTimeEntryByIdHandler(
                _timeEntryRepoMock.Object,
                _projectRepoMock.Object,
                _currentUserServiceMock.Object);
        }

        [Fact]
        public async Task Handle_ExistingTimeEntry_ReturnsResponse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var startedAt = DateTime.UtcNow.AddHours(-2);
            var endedAt = DateTime.UtcNow;

            var timeEntry = new TimeEntry(projectId, startedAt, endedAt, "Coding backend");
            var project = new Project(
                userId,
                "Test Project",
                DevOS.Domain.Entities.ProjectPriority.Medium,
                "Test Description");

            _currentUserServiceMock
                .Setup(s => s.UserId)
                .Returns(userId);

            _projectRepoMock
                .Setup(r => r.GetByIdAsync(projectId, userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(project);

            _timeEntryRepoMock
                .Setup(r => r.GetByIdAsync(timeEntry.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(timeEntry);

            var query = new GetTimeEntryByIdQuery
            {
                Id = timeEntry.Id,
                ProjectId = projectId
            };

            // Act
            var response = await _handler.HandleAsync(query);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(timeEntry.Id, response.Id);
            Assert.Equal(projectId, response.ProjectId);
            Assert.Equal(timeEntry.StartedAt, response.StartedAt);
            Assert.Equal(timeEntry.EndedAt ?? timeEntry.StartedAt, response.EndedAt);
            Assert.Equal("Coding backend", response.Description);

            _timeEntryRepoMock.Verify(
                r => r.GetByIdAsync(timeEntry.Id, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_TimeEntryNotFound_ReturnsNull()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var entryId = Guid.NewGuid();

            var project = new Project(
                userId,
                "Test Project",
                DevOS.Domain.Entities.ProjectPriority.Medium,
                "Test Description");

            _currentUserServiceMock
                .Setup(s => s.UserId)
                .Returns(userId);

            _projectRepoMock
                .Setup(r => r.GetByIdAsync(projectId, userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(project);

            _timeEntryRepoMock
                .Setup(r => r.GetByIdAsync(entryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((TimeEntry?)null);

            var query = new GetTimeEntryByIdQuery
            {
                Id = entryId,
                ProjectId = projectId
            };

            // Act
            var response = await _handler.HandleAsync(query);

            // Assert
            Assert.Null(response);
        }

        [Fact]
        public async Task Handle_UnauthorizedUser_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            _currentUserServiceMock
                .Setup(s => s.UserId)
                .Returns(Guid.Empty);

            var query = new GetTimeEntryByIdQuery
            {
                Id = Guid.NewGuid(),
                ProjectId = Guid.NewGuid()
            };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _handler.HandleAsync(query));
        }

        [Fact]
        public async Task Handle_ProjectNotFound_ReturnsNull()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var entryId = Guid.NewGuid();

            _currentUserServiceMock
                .Setup(s => s.UserId)
                .Returns(userId);

            _projectRepoMock
                .Setup(r => r.GetByIdAsync(projectId, userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Project?)null);

            var query = new GetTimeEntryByIdQuery
            {
                Id = entryId,
                ProjectId = projectId
            };

            // Act
            var response = await _handler.HandleAsync(query);

            // Assert
            Assert.Null(response);
        }
    }
}