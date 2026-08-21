using System.Net;
using System.Text.Json;
using DevOS.Application.Exceptions;
using DevOS.Application.Validation;

namespace DevOS.Api.Exceptions
{
    public class GlobalExceptionHandler
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(
            RequestDelegate next,
            ILogger<GlobalExceptionHandler> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning("Validation failed: {Errors}", string.Join("; ", ex.Errors));
                
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.ContentType = "application/json";

                var response = new
                {
                    error = "Validation failed",
                    errors = ex.Errors
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
            catch (ProjectNotFoundException ex)
            {
                _logger.LogWarning("Project not found: {ProjectId}", ex.ProjectId);
                
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                context.Response.ContentType = "application/json";

                var response = new
                {
                    error = "Project not found"
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
            catch (TaskNotFoundException ex)
            {
                _logger.LogWarning("Task not found: {TaskId}", ex.TaskId);
                
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                context.Response.ContentType = "application/json";

                var response = new
                {
                    error = "Task not found"
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
            catch (TimeEntryNotFoundException ex)
            {
                _logger.LogWarning("Time entry not found: {EntryId}", ex.EntryId);
                
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                context.Response.ContentType = "application/json";

                var response = new
                {
                    error = "Time entry not found"
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid argument: {Message}", ex.Message);
                
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.ContentType = "application/json";

                var response = new
                {
                    error = "Invalid argument",
                    message = ex.Message
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred.");

                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";

                var response = new
                {
                    error = "Internal server error"
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
        }
    }
}