using DevOS.Application.TimeEntries.Commands.CreateTimeEntry;
using DevOS.Application.TimeEntries.Commands.DeleteTimeEntry;
using DevOS.Application.TimeEntries.Commands.UpdateTimeEntry;
using DevOS.Application.TimeEntries.DTOs;
using DevOS.Application.TimeEntries.Queries.GetTimeEntries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevOS.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/projects/{projectId:guid}/time-entries")]
    public class TimeEntriesController : ControllerBase
    {
        private readonly CreateTimeEntryHandler _createTimeEntryHandler;
        private readonly UpdateTimeEntryHandler _updateTimeEntryHandler;
        private readonly DeleteTimeEntryHandler _deleteTimeEntryHandler;
        private readonly GetTimeEntriesHandler _getTimeEntriesHandler;

        public TimeEntriesController(
            CreateTimeEntryHandler createTimeEntryHandler,
            UpdateTimeEntryHandler updateTimeEntryHandler,
            DeleteTimeEntryHandler deleteTimeEntryHandler,
            GetTimeEntriesHandler getTimeEntriesHandler)
        {
            _createTimeEntryHandler = createTimeEntryHandler;
            _updateTimeEntryHandler = updateTimeEntryHandler;
            _deleteTimeEntryHandler = deleteTimeEntryHandler;
            _getTimeEntriesHandler = getTimeEntriesHandler;
        }

        [HttpGet]
        public async Task<ActionResult<List<TimeEntryDto>>> GetTimeEntries(
            [FromRoute] Guid projectId,
            [FromQuery] Guid? taskId,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            CancellationToken cancellationToken)
        {
            var query = new GetTimeEntriesQuery
            {
                ProjectId = projectId,
                TaskId = taskId,
                From = from,
                To = to
            };

            var result = await _getTimeEntriesHandler.HandleAsync(query, cancellationToken);
            if (result == null)
            {
                return NotFound(new { message = $"Project with ID '{projectId}' was not found." });
            }

            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<TimeEntryDto>> Create(
            [FromRoute] Guid projectId,
            [FromBody] CreateTimeEntryCommand command,
            CancellationToken cancellationToken)
        {
            // Передаем параметры через позиционный конструктор record
            var createCommand = new CreateTimeEntryCommand(
                projectId,
                command.StartedAt,
                command.EndedAt,
                command.Description,
                command.TaskId
            );

            var result = await _createTimeEntryHandler.HandleAsync(createCommand, cancellationToken);
            if (result == null)
            {
                return NotFound(new { message = $"Project with ID '{projectId}' was not found." });
            }

            return CreatedAtAction(nameof(GetTimeEntries), new { projectId }, result);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<TimeEntryDto>> Update(
            [FromRoute] Guid projectId,
            [FromRoute] Guid id,
            [FromBody] UpdateTimeEntryCommand command,
            CancellationToken cancellationToken)
        {
            var updateCommand = new UpdateTimeEntryCommand(
                id,
                projectId,
                command.StartedAt,
                command.EndedAt,
                command.Description,
                command.TaskId
            );

            var result = await _updateTimeEntryHandler.HandleAsync(updateCommand, cancellationToken);
            if (result == null)
            {
                return NotFound(new { message = $"TimeEntry with ID '{id}' or Project was not found." });
            }

            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(
            [FromRoute] Guid projectId,
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var command = new DeleteTimeEntryCommand(id, projectId);
            var isDeleted = await _deleteTimeEntryHandler.HandleAsync(command, cancellationToken);

            if (!isDeleted)
            {
                return NotFound(new { message = $"TimeEntry with ID '{id}' or Project was not found." });
            }

            return NoContent();
        }
    }
}