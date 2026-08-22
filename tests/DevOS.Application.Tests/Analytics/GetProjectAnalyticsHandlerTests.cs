using DevOS.Application.Abstractions.Services;
using DevOS.Application.Analytics.Queries;
using Xunit;

namespace DevOS.Application.Tests.Analytics
{
    public class TestCurrentUserServiceForAnalytics : ICurrentUserService
    {
        public Guid UserId { get; set; } = Guid.Empty;
    }

    public class GetProjectAnalyticsHandlerTests
    {
        [Fact]
        public async Task HandleAsync_ShouldThrow_WhenUserNotAuthenticated()
        {
            var currentUserService = new TestCurrentUserServiceForAnalytics { UserId = Guid.Empty };
            var handler = new GetProjectAnalyticsHandler(null!, currentUserService);
            var query = new GetProjectAnalyticsQuery { ProjectId = Guid.NewGuid() };

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.HandleAsync(query));
        }
    }
}