using DevOS.Application.Exceptions;
using DevOS.Application.Abstractions.Repositories;
using DevOS.Application.Projects.Queries.GetProjectById;
using DevOS.Domain.Entities;
using Moq;

namespace DevOS.Application.Tests.Projects.GetProject
{
    public class GetProjectByIdHandlerTests
    {
        [Fact]
        public async Task Handle_ExistingProject_ReturnsResponse()
        {
            // Arrange
            var project = new Project(
                "Test Project",
                ProjectPriority.High,
                "Test Description",
                DateTime.UtcNow.AddDays(10));

            project.UpdateStatus(ProjectStatus.Active);

            var mockRepository = new Mock<IProjectRepository>();

            mockRepository
                .Setup(r => r.GetByIdAsync(
                    project.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(project);

            var handler = new GetProjectByIdHandler(mockRepository.Object);

            var query = new GetProjectByIdQuery
            {
                Id = project.Id
            };

            // Act
            var response = await handler.HandleAsync(query);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(project.Id, response.Id);
            Assert.Equal(project.Name, response.Name);
            Assert.Equal(project.Description, response.Description);
            Assert.Equal(project.Status, response.Status);
            Assert.Equal(project.Priority, response.Priority);
            Assert.Equal(project.Deadline, response.Deadline);
            Assert.Equal(project.CreatedAt, response.CreatedAt);
            Assert.Equal(project.UpdatedAt, response.UpdatedAt);

            mockRepository.Verify(
                r => r.GetByIdAsync(
                    project.Id,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ProjectNotFound_ReturnsNull()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var mockRepository = new Mock<IProjectRepository>();

            mockRepository
                .Setup(r => r.GetByIdAsync(
                    projectId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Project?)null);

            var handler = new GetProjectByIdHandler(mockRepository.Object);

            var query = new GetProjectByIdQuery
            {
                Id = projectId
            };

            // Act & Assert
            var response = await handler.HandleAsync(query);
            Assert.Null(response);

            mockRepository.Verify(
                r => r.GetByIdAsync(
                    projectId,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}

