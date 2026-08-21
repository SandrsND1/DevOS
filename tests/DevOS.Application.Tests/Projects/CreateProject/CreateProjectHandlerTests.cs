using DevOS.Application.Abstractions.Repositories;
using DevOS.Application.Projects.Commands.CreateProject;
using DevOS.Domain.Entities;
using Moq;

namespace DevOS.Application.Tests.Projects.CreateProject
{
    public class CreateProjectHandlerTests
    {
        [Fact]
        public async Task Handle_ValidCommand_CreatesProjectAndReturnsResponse()
        {
            // Arrange
            var mockRepo = new Mock<IProjectRepository>();
            mockRepo
                .Setup(r => r.AddAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var handler = new CreateProjectHandler(mockRepo.Object);

            var command = new CreateProjectCommand
            {
                Name = "DevOS Pet Project",
                Description = "Backend for task and time tracking",
                Priority = ProjectPriority.High,
                Deadline = DateTime.UtcNow.AddDays(30)
            };

            // Act
            var response = await handler.HandleAsync(command);

            // Assert
            Assert.NotNull(response);
            Assert.NotEqual(Guid.Empty, response.Id);
            Assert.Equal(command.Name, response.Name);
            Assert.Equal(command.Description, response.Description);
            Assert.Equal(ProjectStatus.Planning, response.Status);
            Assert.Equal(command.Priority, response.Priority);
            Assert.Equal(command.Deadline, response.Deadline);

            mockRepo.Verify(
                r => r.AddAsync(
                    It.Is<Project>(p => p.Name == command.Name && p.Priority == command.Priority),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task Handle_InvalidName_ThrowsArgumentException(string? name)
        {
            // Arrange
            var mockRepo = new Mock<IProjectRepository>();
            var handler = new CreateProjectHandler(mockRepo.Object);

            var command = new CreateProjectCommand
            {
                Name = name!,
                Priority = ProjectPriority.Medium
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync(command));

            mockRepo.Verify(
                r => r.AddAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
