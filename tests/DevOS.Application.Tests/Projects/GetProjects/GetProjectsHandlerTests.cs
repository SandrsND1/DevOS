using DevOS.Application.Abstractions.Repositories;
using DevOS.Application.Projects.Queries.GetProjects;
using DevOS.Domain.Entities;
using Moq;

namespace DevOS.Application.Tests.Projects.GetProjects
{
    public class GetProjectsHandlerTests
    {
        [Fact]
        public async Task Handle_ValidQuery_ReturnsProjectsWithPagination()
        {
            // Arrange
            var projects = new List<Project>
            {
                new Project(
                    "Project 1",
                    ProjectPriority.High,
                    "Description 1",
                    DateTime.UtcNow.AddDays(10)),

                new Project(
                    "Project 2",
                    ProjectPriority.Low,
                    "Description 2",
                    DateTime.UtcNow.AddDays(20))
            };

            var mockRepository = new Mock<IProjectRepository>();

            mockRepository
                .Setup(r => r.GetTotalCountAsync(
                    null,
                    null,
                    null,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(25);

            mockRepository
                .Setup(r => r.GetPagedAsync(
                    2,
                    10,
                    null,
                    null,
                    null,
                    "CreatedAt",
                    "desc",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(projects);

            var query = new GetProjectsQuery
            {
                Page = 2,
                PageSize = 10
            };
            var handler = new GetProjectsHandler(mockRepository.Object);

            // Act
            var response = await handler.HandleAsync(query);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(2, response.Page);
            Assert.Equal(10, response.PageSize);
            Assert.Equal(25, response.TotalCount);
            Assert.Equal(3, response.TotalPages);
            Assert.Equal(2, response.Items.Count);

            Assert.Equal(projects[0].Id, response.Items[0].Id);
            Assert.Equal(projects[0].Name, response.Items[0].Name);
            Assert.Equal(projects[1].Id, response.Items[1].Id);

            mockRepository.Verify(
                r => r.GetTotalCountAsync(
                    null,
                    null,
                    null,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            mockRepository.Verify(
                r => r.GetPagedAsync(
                    2,
                    10,
                    null,
                    null,
                    null,
                    "CreatedAt",
                    "desc",
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_WithFilters_ParsesAndPassesToRepository()
        {
            // Arrange
            var mockRepository = new Mock<IProjectRepository>();

            mockRepository
                .Setup(r => r.GetTotalCountAsync(
                    ProjectStatus.Active,
                    ProjectPriority.High,
                    "Unity",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(5);

            mockRepository
                .Setup(r => r.GetPagedAsync(
                    1,
                    10,
                    ProjectStatus.Active,
                    ProjectPriority.High,
                    "Unity",
                    "CreatedAt",
                    "desc",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Project>());

            var query = new GetProjectsQuery
            {
                Page = 1,
                PageSize = 10,
                Status = ProjectStatus.Active,
                Priority = ProjectPriority.High,
                Search = "Unity",
                SortBy = "CreatedAt",
                SortDirection = "desc"
            };
            var handler = new GetProjectsHandler(mockRepository.Object);

            // Act
            var response = await handler.HandleAsync(query);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(5, response.TotalCount);
            Assert.Equal(1, response.TotalPages);

            mockRepository.Verify(
                r => r.GetTotalCountAsync(
                    ProjectStatus.Active,
                    ProjectPriority.High,
                    "Unity",
                    It.IsAny<CancellationToken>()),
                Times.Once);

            mockRepository.Verify(
                r => r.GetPagedAsync(
                    1,
                    10,
                    ProjectStatus.Active,
                    ProjectPriority.High,
                    "Unity",
                    "CreatedAt",
                    "desc",
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_InvalidQuery_ThrowsValidationException()
        {
            // Arrange
            var mockRepository = new Mock<IProjectRepository>();

            var query = new GetProjectsQuery
            {
                Page = 0,
                PageSize = 10
            };
            var handler = new GetProjectsHandler(mockRepository.Object);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => handler.HandleAsync(query));

            mockRepository.Verify(
                r => r.GetTotalCountAsync(
                    It.IsAny<ProjectStatus?>(),
                    It.IsAny<ProjectPriority?>(),
                    It.IsAny<string?>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            mockRepository.Verify(
                r => r.GetPagedAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<ProjectStatus?>(),
                    It.IsAny<ProjectPriority?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_ValidQuery_MapsAllFields()
        {
            // Arrange
            var project = new Project(
                "Test Project",
                ProjectPriority.Critical,
                "Test Description",
                DateTime.UtcNow.AddDays(15));

            project.UpdateStatus(ProjectStatus.Active);

            var mockRepository = new Mock<IProjectRepository>();

            mockRepository
                .Setup(r => r.GetTotalCountAsync(
                    null,
                    null,
                    null,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            mockRepository
                .Setup(r => r.GetPagedAsync(
                    1,
                    10,
                    null,
                    null,
                    null,
                    "CreatedAt",
                    "desc",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Project> { project });

            var query = new GetProjectsQuery
            {
                Page = 1,
                PageSize = 10
            };
            var handler = new GetProjectsHandler(mockRepository.Object);

            // Act
            var response = await handler.HandleAsync(query);

            // Assert
            Assert.NotNull(response);
            Assert.Single(response.Items);

            var item = response.Items[0];
            Assert.Equal(project.Id, item.Id);
            Assert.Equal(project.Name, item.Name);
            Assert.Equal(project.Status, item.Status);
            Assert.Equal(project.Priority, item.Priority);
            Assert.Equal(project.Deadline, item.Deadline);
            Assert.Equal(project.CreatedAt, item.CreatedAt);
        }

        [Fact]
        public async Task Handle_WithSorting_PassesToRepository()
        {
            // Arrange
            var mockRepository = new Mock<IProjectRepository>();

            mockRepository
                .Setup(r => r.GetTotalCountAsync(
                    null,
                    null,
                    null,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(10);

            mockRepository
                .Setup(r => r.GetPagedAsync(
                    1,
                    10,
                    null,
                    null,
                    null,
                    "Name",
                    "asc",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Project>());

            var query = new GetProjectsQuery
            {
                Page = 1,
                PageSize = 10,
                SortBy = "Name",
                SortDirection = "asc"
            };
            var handler = new GetProjectsHandler(mockRepository.Object);

            // Act
            var response = await handler.HandleAsync(query);

            // Assert
            Assert.NotNull(response);

            mockRepository.Verify(
                r => r.GetPagedAsync(
                    1,
                    10,
                    null,
                    null,
                    null,
                    "Name",
                    "asc",
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}




