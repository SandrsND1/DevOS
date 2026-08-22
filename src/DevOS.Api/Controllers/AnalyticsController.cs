using DevOS.Application.Analytics.DTOs;
using DevOS.Application.Analytics.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevOS.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/projects/{projectId:guid}/analytics")]
    public class AnalyticsController : ControllerBase
    {
        private readonly GetProjectAnalyticsHandler _getProjectAnalyticsHandler;

        public AnalyticsController(GetProjectAnalyticsHandler getProjectAnalyticsHandler)
        {
            _getProjectAnalyticsHandler = getProjectAnalyticsHandler;
        }

        [HttpGet]
        public async Task<ActionResult<ProjectAnalyticsDto>> GetAnalytics(
            [FromRoute] Guid projectId,
            CancellationToken cancellationToken)
        {
            var query = new GetProjectAnalyticsQuery { ProjectId = projectId };
            var result = await _getProjectAnalyticsHandler.HandleAsync(query, cancellationToken);

            if (result == null)
            {
                return NotFound(new { message = $"Project with ID '{projectId}' was not found." });
            }

            return Ok(result);
        }
    }
}