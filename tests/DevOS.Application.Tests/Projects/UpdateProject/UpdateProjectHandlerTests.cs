using DevOS.Application.Exceptions;
using DevOS.Application.Abstractions.Repositories;
using DevOS.Application.Projects.Commands.UpdateProject;
using DevOS.Domain.Entities;
using Moq;

namespace DevOS.Application.Tests.Projects.UpdateProject
{
    public class UpdateProjectHandlerTests
    {
        [Fact]
        public async Task Handle_ValidCommand_UpdatesProjectAndReturnsResponse()
        {
            // Arrange
            var existingProject = new Project(
                "Old Name",
                ProjectPriority.Low,
                "Old Description",
                DateTime.UtcNow.AddDays(10));

            var mockRepository = new Mock<IProjectRepository>();

            mockRepository
                .Setup(r => r.GetByIdAsync(existingProject.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingProject);

            mockRepository
                .Setup(r => r.UpdateAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var command = new UpdateProjectCommand
            {
                Id = existingProject.Id,
                Name = "Updated Name",
                Description = "Updated Description",
                Status = ProjectStatus.Active,
                Priority = ProjectPriority.High,
                Deadline = DateTime.UtcNow.AddDays(20)
            };

            var handler = new UpdateProjectHandler(mockRepository.Object);

            // Act
            var response = await handler.HandleAsync(command);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(existingProject.Id, response.Id);
            Assert.Equal(command.Name, response.Name);
            Assert.Equal(command.Description, response.Description);
            Assert.Equal(command.Status, response.Status);
            Assert.Equal(command.Priority, response.Priority);
            Assert.Equal(command.Deadline, response.Deadline);

            mockRepository.Verify(
                r => r.UpdateAsync(
                    It.Is<Project>(p =>
                        p.Id == existingProject.Id &&
                        p.Name == command.Name &&
                        p.Description == command.Description &&
                        p.Status == command.Status &&
                        p.Priority == command.Priority &&
                        p.Deadline == command.Deadline),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_InvalidCommand_ThrowsValidationException()
        {
            // Arrange
            var mockRepository = new Mock<IProjectRepository>();

            var command = new UpdateProjectCommand
            {
                Id = Guid.NewGuid(),
                Name = "   ",
                Description = null,
                Status = ProjectStatus.Planning,
                Priority = ProjectPriority.Medium,
                Deadline = null
            };

            var handler = new UpdateProjectHandler(mockRepository.Object);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => handler.HandleAsync(command));

            mockRepository.Verify(
                r => r.GetByIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            mockRepository.Verify(
                r => r.UpdateAsync(
                    It.IsAny<Project>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_ProjectNotFound_ThrowsProjectNotFoundException()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var mockRepository = new Mock<IProjectRepository>();

            mockRepository
                .Setup(r => r.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Project?)null);

            var command = new UpdateProjectCommand
            {
                Id = projectId,
                Name = "Valid Name",
                Description = "Valid Description",
                Status = ProjectStatus.Active,
                Priority = ProjectPriority.High,
                Deadline = DateTime.UtcNow.AddDays(20)
            };

            var handler = new UpdateProjectHandler(mockRepository.Object);

            // Act & Assert
            await Assert.ThrowsAsync<ProjectNotFoundException>(
                () => handler.HandleAsync(command));

            mockRepository.Verify(
                r => r.GetByIdAsync(
                    projectId,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            mockRepository.Verify(
                r => r.UpdateAsync(
                    It.IsAny<Project>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}

