using DevOS.Application.Tasks.CreateTask;
using DevOS.Application.Tasks.DeleteTask;
using DevOS.Application.Tasks.GetTask;
using DevOS.Application.Tasks.GetTasks;
using DevOS.Application.Tasks.UpdateTask;
using Microsoft.AspNetCore.Mvc;

namespace DevOS.Api.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId:guid}/tasks")]
    public class TasksController : ControllerBase
    {
        private readonly CreateTaskHandler _createTaskHandler;
        private readonly GetTaskHandler _getTaskHandler;
        private readonly GetTasksHandler _getTasksHandler;
        private readonly UpdateTaskHandler _updateTaskHandler;
        private readonly DeleteTaskHandler _deleteTaskHandler;

        public TasksController(
            CreateTaskHandler createTaskHandler,
            GetTaskHandler getTaskHandler,
            GetTasksHandler getTasksHandler,
            UpdateTaskHandler updateTaskHandler,
            DeleteTaskHandler deleteTaskHandler)
        {
            _createTaskHandler = createTaskHandler;
            _getTaskHandler = getTaskHandler;
            _getTasksHandler = getTasksHandler;
            _updateTaskHandler = updateTaskHandler;
            _deleteTaskHandler = deleteTaskHandler;
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            Guid projectId,
            [FromBody] CreateTaskCommand command,
            CancellationToken cancellationToken)
        {
            var createCommand = new CreateTaskCommand
            {
                ProjectId = projectId,
                Title = command.Title,
                Description = command.Description,
                Priority = command.Priority,
                EstimatedMinutes = command.EstimatedMinutes,
                Deadline = command.Deadline
            };

            var result = await _createTaskHandler.HandleAsync(createCommand, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { projectId, taskId = result.Id }, result);
        }

        [HttpGet("{taskId:guid}")]
        public async Task<IActionResult> GetById(
            Guid projectId,
            Guid taskId,
            CancellationToken cancellationToken)
        {
            var query = new GetTaskQuery
            {
                ProjectId = projectId,
                TaskId = taskId
            };

            var result = await _getTaskHandler.HandleAsync(query, cancellationToken);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetList(
            Guid projectId,
            [FromQuery] GetTasksQuery query,
            CancellationToken cancellationToken)
        {
            var getQuery = new GetTasksQuery
            {
                ProjectId = projectId,
                Page = query.Page,
                PageSize = query.PageSize,
                Status = query.Status,
                Priority = query.Priority,
                Search = query.Search,
                SortBy = query.SortBy,
                SortDirection = query.SortDirection
            };

            var result = await _getTasksHandler.HandleAsync(getQuery, cancellationToken);
            return Ok(result);
        }

        [HttpPut("{taskId:guid}")]
        public async Task<IActionResult> Update(
            Guid projectId,
            Guid taskId,
            [FromBody] UpdateTaskCommand command,
            CancellationToken cancellationToken)
        {
            var updateCommand = new UpdateTaskCommand
            {
                TaskId = taskId,
                ProjectId = projectId,
                Title = command.Title,
                Description = command.Description,
                Status = command.Status,
                Priority = command.Priority,
                EstimatedMinutes = command.EstimatedMinutes,
                Deadline = command.Deadline
            };

            var result = await _updateTaskHandler.HandleAsync(updateCommand, cancellationToken);
            return Ok(result);
        }

        [HttpDelete("{taskId:guid}")]
        public async Task<IActionResult> Delete(
            Guid projectId,
            Guid taskId,
            CancellationToken cancellationToken)
        {
            var command = new DeleteTaskCommand
            {
                ProjectId = projectId,
                TaskId = taskId
            };

            await _deleteTaskHandler.HandleAsync(command, cancellationToken);
            return NoContent();
        }
    }
}
