using DevOS.Application.Abstractions.Repositories;
using DevOS.Application.Exceptions;
using DevOS.Application.Projects.Commands.ChangeProjectStatus;
using DevOS.Domain.Entities;
using Moq;

namespace DevOS.Application.Tests.Projects.ChangeProjectStatus
{
    public class ChangeProjectStatusHandlerTests
    {
        [Fact]
        public async Task Handle_ExistingProject_UpdatesStatusAndReturnsResponse()
        {
            // Arrange
            var project = new Project("Test Project", ProjectPriority.High);
            var mockRepo = new Mock<IProjectRepository>();
            mockRepo
                .Setup(r => r.GetByIdAsync(project.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(project);

            mockRepo
                .Setup(r => r.UpdateAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var handler = new ChangeProjectStatusHandler(mockRepo.Object);

            var command = new ChangeProjectStatusCommand
            {
                Id = project.Id,
                Status = ProjectStatus.Active
            };

            // Act
            var response = await handler.HandleAsync(command);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(project.Id, response.Id);
            Assert.Equal(ProjectStatus.Active, response.Status);

            mockRepo.Verify(
                r => r.UpdateAsync(
                    It.Is<Project>(p => p.Id == project.Id && p.Status == ProjectStatus.Active),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ProjectNotFound_ThrowsProjectNotFoundException()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var mockRepo = new Mock<IProjectRepository>();
            mockRepo
                .Setup(r => r.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Project?)null);

            var handler = new ChangeProjectStatusHandler(mockRepo.Object);

            var command = new ChangeProjectStatusCommand
            {
                Id = projectId,
                Status = ProjectStatus.Completed
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ProjectNotFoundException>(
                () => handler.HandleAsync(command));

            Assert.Equal(projectId, ex.ProjectId);

            mockRepo.Verify(
                r => r.UpdateAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
