using DevOS.Application.Exceptions;
using DevOS.Application.Abstractions.Repositories;
using DevOS.Application.Projects.Commands.DeleteProject;
using DevOS.Domain.Entities;
using Moq;

namespace DevOS.Application.Tests.Projects.DeleteProject
{
    public class DeleteProjectHandlerTests
    {
        [Fact]
        public async Task Handle_ExistingProject_DeletesProject()
        {
            // Arrange
            var project = new Project(
                "Test Project",
                ProjectPriority.High,
                "Test Description",
                DateTime.UtcNow.AddDays(10));

            var mockRepository = new Mock<IProjectRepository>();

            mockRepository
                .Setup(r => r.GetByIdAsync(
                    project.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(project);

            mockRepository
                .Setup(r => r.DeleteAsync(
                    project,
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var handler = new DeleteProjectHandler(mockRepository.Object);

            var command = new DeleteProjectCommand
            {
                Id = project.Id
            };

            // Act
            await handler.HandleAsync(command);

            // Assert
            mockRepository.Verify(
                r => r.GetByIdAsync(
                    project.Id,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            mockRepository.Verify(
                r => r.DeleteAsync(
                    project,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ProjectNotFound_ThrowsProjectNotFoundException()
        {
            // Arrange
            var projectId = Guid.NewGuid();

            var mockRepository = new Mock<IProjectRepository>();

            mockRepository
                .Setup(r => r.GetByIdAsync(
                    projectId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Project?)null);

            var handler = new DeleteProjectHandler(mockRepository.Object);

            var command = new DeleteProjectCommand
            {
                Id = projectId
            };

            // Act & Assert
            await Assert.ThrowsAsync<ProjectNotFoundException>(
                () => handler.HandleAsync(command));

            mockRepository.Verify(
                r => r.GetByIdAsync(
                    projectId,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            mockRepository.Verify(
                r => r.DeleteAsync(
                    It.IsAny<Project>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
