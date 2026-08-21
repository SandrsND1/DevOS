using DevOS.Application.TimeEntries.Commands.CreateTimeEntry;
using DevOS.Application.TimeEntries.Commands.DeleteTimeEntry;
using DevOS.Application.TimeEntries.Commands.UpdateTimeEntry;
using DevOS.Application.TimeEntries.Queries.GetTimeEntries;
using DevOS.Application.TimeEntries.Queries.GetTimeEntryById;
using Microsoft.AspNetCore.Mvc;

namespace DevOS.Api.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId:guid}/time-entries")]
    public class TimeEntriesController : ControllerBase
    {
        private readonly CreateTimeEntryHandler _createTimeEntryHandler;
        private readonly GetTimeEntryByIdHandler _getTimeEntryByIdHandler;
        private readonly GetTimeEntriesHandler _getTimeEntriesHandler;
        private readonly UpdateTimeEntryHandler _updateTimeEntryHandler;
        private readonly DeleteTimeEntryHandler _deleteTimeEntryHandler;

        public TimeEntriesController(
            CreateTimeEntryHandler createTimeEntryHandler,
            GetTimeEntryByIdHandler getTimeEntryByIdHandler,
            GetTimeEntriesHandler getTimeEntriesHandler,
            UpdateTimeEntryHandler updateTimeEntryHandler,
            DeleteTimeEntryHandler deleteTimeEntryHandler)
        {
            _createTimeEntryHandler = createTimeEntryHandler;
            _getTimeEntryByIdHandler = getTimeEntryByIdHandler;
            _getTimeEntriesHandler = getTimeEntriesHandler;
            _updateTimeEntryHandler = updateTimeEntryHandler;
            _deleteTimeEntryHandler = deleteTimeEntryHandler;
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            Guid projectId,
            [FromBody] CreateTimeEntryCommand command,
            CancellationToken cancellationToken)
        {
            var createCommand = new CreateTimeEntryCommand
            {
                ProjectId = projectId,
                TaskId = command.TaskId,
                StartedAt = command.StartedAt,
                EndedAt = command.EndedAt,
                Description = command.Description
            };

            var result = await _createTimeEntryHandler.HandleAsync(createCommand, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { projectId, entryId = result.Id }, result);
        }

        [HttpGet("{entryId:guid}")]
        public async Task<IActionResult> GetById(
            Guid projectId,
            Guid entryId,
            CancellationToken cancellationToken)
        {
            var query = new GetTimeEntryByIdQuery
            {
                ProjectId = projectId,
                EntryId = entryId
            };

            var result = await _getTimeEntryByIdHandler.HandleAsync(query, cancellationToken);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetList(
            Guid projectId,
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
            return Ok(result);
        }

        [HttpPut("{entryId:guid}")]
        public async Task<IActionResult> Update(
            Guid projectId,
            Guid entryId,
            [FromBody] UpdateTimeEntryCommand command,
            CancellationToken cancellationToken)
        {
            var updateCommand = new UpdateTimeEntryCommand
            {
                EntryId = entryId,
                ProjectId = projectId,
                TaskId = command.TaskId,
                StartedAt = command.StartedAt,
                EndedAt = command.EndedAt,
                Description = command.Description
            };

            var result = await _updateTimeEntryHandler.HandleAsync(updateCommand, cancellationToken);
            return Ok(result);
        }

        [HttpDelete("{entryId:guid}")]
        public async Task<IActionResult> Delete(
            Guid projectId,
            Guid entryId,
            CancellationToken cancellationToken)
        {
            var command = new DeleteTimeEntryCommand
            {
                ProjectId = projectId,
                EntryId = entryId
            };

            await _deleteTimeEntryHandler.HandleAsync(command, cancellationToken);
            return NoContent();
        }
    }
}
