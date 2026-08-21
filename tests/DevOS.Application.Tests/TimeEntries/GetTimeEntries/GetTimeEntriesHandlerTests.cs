using DevOS.Application.Abstractions.Repositories;
using DevOS.Application.Exceptions;
using DevOS.Application.TimeEntries;
using DevOS.Application.TimeEntries.Queries.GetTimeEntries;
using DevOS.Application.Validation;
using DevOS.Domain.Entities;
using Moq;

namespace DevOS.Application.Tests.TimeEntries.GetTimeEntries
{
    public class GetTimeEntriesHandlerTests
    {
        [Fact]
        public async Task Handle_GetAllForProject_ReturnsEntries()
        {
            // Arrange
            var project = new Project("Test Project", ProjectPriority.Medium);
            var mockProjectRepo = new Mock<IProjectRepository>();
            mockProjectRepo
                .Setup(r => r.GetByIdAsync(project.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(project);

            var entry1 = new TimeEntry(project.Id, DateTime.UtcNow.AddHours(-3), DateTime.UtcNow.AddHours(-2), "Entry 1");
            var entry2 = new TimeEntry(project.Id, DateTime.UtcNow.AddHours(-1), DateTime.UtcNow, "Entry 2");

            var mockTimeEntryRepo = new Mock<ITimeEntryRepository>();
            mockTimeEntryRepo
                .Setup(r => r.GetAllByProjectIdAsync(project.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<TimeEntry> { entry1, entry2 });

            var validator = new GetTimeEntriesValidator();
            var handler = new GetTimeEntriesHandler(mockTimeEntryRepo.Object, mockProjectRepo.Object, validator);

            var query = new GetTimeEntriesQuery
            {
                ProjectId = project.Id
            };

            // Act
            var result = await handler.HandleAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(entry1.Id, result[0].Id);
            Assert.Equal(entry2.Id, result[1].Id);

            mockTimeEntryRepo.Verify(
                r => r.GetAllByProjectIdAsync(project.Id, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_GetByPeriod_CallsGetByPeriodAsync()
        {
            // Arrange
            var project = new Project("Test Project", ProjectPriority.Medium);
            var mockProjectRepo = new Mock<IProjectRepository>();
            mockProjectRepo
                .Setup(r => r.GetByIdAsync(project.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(project);

            var from = DateTime.UtcNow.AddDays(-7);
            var to = DateTime.UtcNow;

            var entry = new TimeEntry(project.Id, DateTime.UtcNow.AddDays(-3), DateTime.UtcNow.AddDays(-3).AddHours(2), "Period Entry");

            var mockTimeEntryRepo = new Mock<ITimeEntryRepository>();
            mockTimeEntryRepo
                .Setup(r => r.GetByPeriodAsync(project.Id, from, to, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<TimeEntry> { entry });

            var validator = new GetTimeEntriesValidator();
            var handler = new GetTimeEntriesHandler(mockTimeEntryRepo.Object, mockProjectRepo.Object, validator);

            var query = new GetTimeEntriesQuery
            {
                ProjectId = project.Id,
                From = from,
                To = to
            };

            // Act
            var result = await handler.HandleAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(entry.Id, result[0].Id);

            mockTimeEntryRepo.Verify(
                r => r.GetByPeriodAsync(project.Id, from, to, It.IsAny<CancellationToken>()),
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

            var mockTimeEntryRepo = new Mock<ITimeEntryRepository>();
            var validator = new GetTimeEntriesValidator();
            var handler = new GetTimeEntriesHandler(mockTimeEntryRepo.Object, mockProjectRepo.Object, validator);

            var query = new GetTimeEntriesQuery { ProjectId = projectId };

            // Act & Assert
            await Assert.ThrowsAsync<ProjectNotFoundException>(() => handler.HandleAsync(query));
        }

        [Fact]
        public async Task Handle_InvalidPeriod_ThrowsValidationException()
        {
            // Arrange
            var mockProjectRepo = new Mock<IProjectRepository>();
            var mockTimeEntryRepo = new Mock<ITimeEntryRepository>();
            var validator = new GetTimeEntriesValidator();
            var handler = new GetTimeEntriesHandler(mockTimeEntryRepo.Object, mockProjectRepo.Object, validator);

            var query = new GetTimeEntriesQuery
            {
                ProjectId = Guid.NewGuid(),
                From = DateTime.UtcNow,
                To = DateTime.UtcNow.AddHours(-1) // Invalid range
            };

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => handler.HandleAsync(query));
        }
    }
}
