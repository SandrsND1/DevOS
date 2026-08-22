using DevOS.Application.Abstractions.Services;
using DevOS.Application.Projects.Queries.GetProjectById;
using Xunit;

namespace DevOS.Application.Tests.Projects.GetProject
{
    public class TestCurrentUserService : ICurrentUserService
    {
        public Guid UserId { get; set; } = Guid.Empty;
    }

    public class GetProjectHandlerTests
    {
        [Fact]
        public async Task HandleAsync_ShouldThrow_WhenUserNotAuthenticated()
        {
            // Arrange
            var currentUserService = new TestCurrentUserService { UserId = Guid.Empty };
            var handler = new GetProjectByIdHandler(null!, currentUserService);
            var query = new GetProjectByIdQuery { Id = Guid.NewGuid() };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.HandleAsync(query));
        }
    }
}