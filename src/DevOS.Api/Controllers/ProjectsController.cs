using DevOS.Application.Projects.Commands.ChangeProjectStatus;
using DevOS.Application.Projects.Commands.CreateProject;
using DevOS.Application.Projects.Commands.DeleteProject;
using DevOS.Application.Projects.Commands.UpdateProject;
using DevOS.Application.Projects.DTOs;
using DevOS.Application.Projects.Queries.GetProjectById;
using DevOS.Application.Projects.Queries.GetProjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevOS.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectsController : ControllerBase
    {
        private readonly CreateProjectHandler _createProjectHandler;
        private readonly GetProjectByIdHandler _getProjectByIdHandler;
        private readonly GetProjectsHandler _getProjectsHandler;
        private readonly UpdateProjectHandler _updateProjectHandler;
        private readonly DeleteProjectHandler _deleteProjectHandler;
        private readonly ChangeProjectStatusHandler _changeProjectStatusHandler;

        public ProjectsController(
            CreateProjectHandler createProjectHandler,
            GetProjectByIdHandler getProjectByIdHandler,
            GetProjectsHandler getProjectsHandler,
            UpdateProjectHandler updateProjectHandler,
            DeleteProjectHandler deleteProjectHandler,
            ChangeProjectStatusHandler changeProjectStatusHandler)
        {
            _createProjectHandler = createProjectHandler;
            _getProjectByIdHandler = getProjectByIdHandler;
            _getProjectsHandler = getProjectsHandler;
            _updateProjectHandler = updateProjectHandler;
            _deleteProjectHandler = deleteProjectHandler;
            _changeProjectStatusHandler = changeProjectStatusHandler;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<ProjectListItemDto>>> GetProjects(
            [FromQuery] GetProjectsQuery query,
            CancellationToken cancellationToken)
        {
            var result = await _getProjectsHandler.HandleAsync(query, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ProjectDto>> GetById(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var query = new GetProjectByIdQuery { Id = id };
            var result = await _getProjectByIdHandler.HandleAsync(query, cancellationToken);

            if (result == null)
            {
                return NotFound(new { message = $"Project with ID '{id}' was not found." });
            }

            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<ProjectDto>> Create(
            [FromBody] CreateProjectCommand command,
            CancellationToken cancellationToken)
        {
            var result = await _createProjectHandler.HandleAsync(command, cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Id },
                result);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ProjectDto>> Update(
            [FromRoute] Guid id,
            [FromBody] UpdateProjectCommand command,
            CancellationToken cancellationToken)
        {
            var updateCommand = new UpdateProjectCommand
            {
                Id = id,
                Name = command.Name,
                Description = command.Description,
                Priority = command.Priority,
                Status = command.Status,
                Deadline = command.Deadline
            };

            var result = await _updateProjectHandler.HandleAsync(updateCommand, cancellationToken);

            if (result == null)
            {
                return NotFound(new { message = $"Project with ID '{id}' was not found." });
            }

            return Ok(result);
        }

        [HttpPatch("{id:guid}/status")]
        public async Task<ActionResult<ProjectDto>> ChangeStatus(
            [FromRoute] Guid id,
            [FromBody] ChangeProjectStatusCommand command,
            CancellationToken cancellationToken)
        {
            var changeStatusCommand = new ChangeProjectStatusCommand
            {
                Id = id,
                Status = command.Status
            };

            var result = await _changeProjectStatusHandler.HandleAsync(changeStatusCommand, cancellationToken);

            if (result == null)
            {
                return NotFound(new { message = $"Project with ID '{id}' was not found." });
            }

            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var command = new DeleteProjectCommand { Id = id };
            var isDeleted = await _deleteProjectHandler.HandleAsync(command, cancellationToken);

            if (!isDeleted)
            {
                return NotFound(new { message = $"Project with ID '{id}' was not found." });
            }

            return NoContent();
        }
    }
}