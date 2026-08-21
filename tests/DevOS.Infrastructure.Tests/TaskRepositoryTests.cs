using DevOS.Domain.Entities;
using DevOS.Infrastructure.Persistence;
using DevOS.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace DevOS.Infrastructure.Tests
{
    public class TaskRepositoryTests : IAsyncLifetime
    {
        private readonly PostgreSqlContainer _postgres;

        public TaskRepositoryTests()
        {
            _postgres = new PostgreSqlBuilder("postgres:16")
                .WithDatabase("devos_task_test")
                .WithUsername("devos")
                .WithPassword("devos_password")
                .Build();
        }

        public async Task InitializeAsync()
        {
            await _postgres.StartAsync();

            var options = new DbContextOptionsBuilder<DevOsDbContext>()
                .UseNpgsql(_postgres.GetConnectionString())
                .Options;

            using var context = new DevOsDbContext(options);
            await context.Database.MigrateAsync();
        }

        public async Task DisposeAsync()
        {
            await _postgres.DisposeAsync();
        }

        private DevOsDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<DevOsDbContext>()
                .UseNpgsql(_postgres.GetConnectionString())
                .Options;

            return new DevOsDbContext(options);
        }

        [Fact]
        public async Task AddAsync_ValidTask_PersistsTask()
        {
            // Arrange
            var project = new Project("Task Test Project", ProjectPriority.High);
            var projectRepo = new ProjectRepository(CreateContext());
            await projectRepo.AddAsync(project);

            var task = new DevTask(
                project.Id,
                "Setup CI/CD Pipeline",
                TaskPriority.High,
                "Configure GitHub Actions workflow",
                180,
                DateTime.UtcNow.AddDays(14));

            var taskRepo = new TaskRepository(CreateContext());

            // Act
            await taskRepo.AddAsync(task);

            // Assert
            using var verifyContext = CreateContext();
            var persisted = await verifyContext.DevTasks
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == task.Id);

            Assert.NotNull(persisted);
            Assert.Equal(task.Id, persisted.Id);
            Assert.Equal(project.Id, persisted.ProjectId);
            Assert.Equal("Setup CI/CD Pipeline", persisted.Title);
            Assert.Equal("Configure GitHub Actions workflow", persisted.Description);
            Assert.Equal(DevTaskStatus.Todo, persisted.Status);
            Assert.Equal(TaskPriority.High, persisted.Priority);
            Assert.Equal(180, persisted.EstimatedMinutes);
            Assert.Null(persisted.CompletedAt);
            Assert.True(Math.Abs((task.CreatedAt - persisted.CreatedAt).TotalMilliseconds) < 1);
            Assert.True(Math.Abs((task.UpdatedAt - persisted.UpdatedAt).TotalMilliseconds) < 1);
        }

        [Fact]
        public async Task GetByIdAsync_ExistingTask_ReturnsTask()
        {
            // Arrange
            var project = new Project("GetById Project", ProjectPriority.Medium);
            var projectRepo = new ProjectRepository(CreateContext());
            await projectRepo.AddAsync(project);

            var task = new DevTask(project.Id, "GetById Task", TaskPriority.Medium);
            var taskRepo = new TaskRepository(CreateContext());
            await taskRepo.AddAsync(task);

            // Act
            var result = await taskRepo.GetByIdAsync(task.Id, project.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(task.Id, result.Id);
            Assert.Equal(project.Id, result.ProjectId);
            Assert.Equal("GetById Task", result.Title);
        }

        [Fact]
        public async Task GetByIdAsync_TaskBelongsToAnotherProject_ReturnsNull()
        {
            // Arrange
            var project1 = new Project("Project 1", ProjectPriority.Low);
            var project2 = new Project("Project 2", ProjectPriority.Low);
            var projectRepo = new ProjectRepository(CreateContext());
            await projectRepo.AddAsync(project1);
            await projectRepo.AddAsync(project2);

            var task = new DevTask(project1.Id, "Task Project 1", TaskPriority.Low);
            var taskRepo = new TaskRepository(CreateContext());
            await taskRepo.AddAsync(task);

            // Act
            var result = await taskRepo.GetByIdAsync(task.Id, project2.Id);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistingId_ReturnsNull()
        {
            // Arrange
            var project = new Project("Project", ProjectPriority.Medium);
            var projectRepo = new ProjectRepository(CreateContext());
            await projectRepo.AddAsync(project);

            var taskRepo = new TaskRepository(CreateContext());

            // Act
            var result = await taskRepo.GetByIdAsync(Guid.NewGuid(), project.Id);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllByProjectIdAsync_ReturnsOnlyProjectTasks()
        {
            // Arrange
            var project1 = new Project("Project 1", ProjectPriority.Medium);
            var project2 = new Project("Project 2", ProjectPriority.Medium);
            var projectRepo = new ProjectRepository(CreateContext());
            await projectRepo.AddAsync(project1);
            await projectRepo.AddAsync(project2);

            var task1 = new DevTask(project1.Id, "Task P1-1", TaskPriority.Low);
            var task2 = new DevTask(project1.Id, "Task P1-2", TaskPriority.High);
            var task3 = new DevTask(project2.Id, "Task P2-1", TaskPriority.Critical);

            var taskRepo = new TaskRepository(CreateContext());
            await taskRepo.AddAsync(task1);
            await taskRepo.AddAsync(task2);
            await taskRepo.AddAsync(task3);

            // Act
            var p1Tasks = await taskRepo.GetAllByProjectIdAsync(project1.Id);

            // Assert
            Assert.Equal(2, p1Tasks.Count);
            Assert.Contains(p1Tasks, t => t.Id == task1.Id);
            Assert.Contains(p1Tasks, t => t.Id == task2.Id);
            Assert.DoesNotContain(p1Tasks, t => t.Id == task3.Id);
        }

        [Fact]
        public async Task UpdateAsync_ExistingTask_UpdatesTask()
        {
            // Arrange
            var project = new Project("Update Project", ProjectPriority.Medium);
            var projectRepo = new ProjectRepository(CreateContext());
            await projectRepo.AddAsync(project);

            var task = new DevTask(project.Id, "Original Title", TaskPriority.Low);
            var taskRepo = new TaskRepository(CreateContext());
            await taskRepo.AddAsync(task);

            task.UpdateTitle("Updated Title");
            task.UpdateDescription("Updated Description");
            task.UpdatePriority(TaskPriority.Critical);
            task.UpdateStatus(DevTaskStatus.Completed);
            task.UpdateEstimatedMinutes(240);

            // Act
            await taskRepo.UpdateAsync(task);

            // Assert
            using var verifyContext = CreateContext();
            var updated = await verifyContext.DevTasks
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == task.Id);

            Assert.NotNull(updated);
            Assert.Equal("Updated Title", updated.Title);
            Assert.Equal("Updated Description", updated.Description);
            Assert.Equal(TaskPriority.Critical, updated.Priority);
            Assert.Equal(DevTaskStatus.Completed, updated.Status);
            Assert.NotNull(updated.CompletedAt);
            Assert.Equal(240, updated.EstimatedMinutes);
        }

        [Fact]
        public async Task DeleteAsync_ExistingTask_RemovesTask()
        {
            // Arrange
            var project = new Project("Delete Task Project", ProjectPriority.Medium);
            var projectRepo = new ProjectRepository(CreateContext());
            await projectRepo.AddAsync(project);

            var task = new DevTask(project.Id, "Task To Delete", TaskPriority.Medium);
            var taskRepo = new TaskRepository(CreateContext());
            await taskRepo.AddAsync(task);

            // Act
            await taskRepo.DeleteAsync(task);

            // Assert
            using var verifyContext = CreateContext();
            var deleted = await verifyContext.DevTasks
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == task.Id);

            Assert.Null(deleted);
        }

        [Fact]
        public async Task GetTotalCountAsync_ReturnsFilteredCount()
        {
            // Arrange
            var project = new Project("Filter Project", ProjectPriority.Medium);
            var projectRepo = new ProjectRepository(CreateContext());
            await projectRepo.AddAsync(project);

            var task1 = new DevTask(project.Id, "Bug Login", TaskPriority.High);
            task1.UpdateStatus(DevTaskStatus.InProgress);

            var task2 = new DevTask(project.Id, "Feature Registration", TaskPriority.High);

            var task3 = new DevTask(project.Id, "Bug Dashboard", TaskPriority.Low);
            task3.UpdateStatus(DevTaskStatus.InProgress);

            var taskRepo = new TaskRepository(CreateContext());
            await taskRepo.AddAsync(task1);
            await taskRepo.AddAsync(task2);
            await taskRepo.AddAsync(task3);

            // Act
            var totalCount = await taskRepo.GetTotalCountAsync(project.Id, null, null, null);
            var highCount = await taskRepo.GetTotalCountAsync(project.Id, null, TaskPriority.High, null);
            var inProgressCount = await taskRepo.GetTotalCountAsync(project.Id, DevTaskStatus.InProgress, null, null);
            var bugCount = await taskRepo.GetTotalCountAsync(project.Id, null, null, "Bug");

            // Assert
            Assert.Equal(3, totalCount);
            Assert.Equal(2, highCount);
            Assert.Equal(2, inProgressCount);
            Assert.Equal(2, bugCount);
        }

        [Fact]
        public async Task GetPagedAsync_ReturnsPaginatedTasks()
        {
            // Arrange
            var project = new Project("Paged Project", ProjectPriority.Medium);
            var projectRepo = new ProjectRepository(CreateContext());
            await projectRepo.AddAsync(project);

            var taskRepo = new TaskRepository(CreateContext());
            for (int i = 1; i <= 5; i++)
            {
                var task = new DevTask(project.Id, $"Task {i}", TaskPriority.Medium);
                await taskRepo.AddAsync(task);
                await Task.Delay(10);
            }

            // Act
            var page1 = await taskRepo.GetPagedAsync(project.Id, 1, 2, null, null, null, "CreatedAt", "desc");
            var page2 = await taskRepo.GetPagedAsync(project.Id, 2, 2, null, null, null, "CreatedAt", "desc");
            var page3 = await taskRepo.GetPagedAsync(project.Id, 3, 2, null, null, null, "CreatedAt", "desc");

            // Assert
            Assert.Equal(2, page1.Count);
            Assert.Equal(2, page2.Count);
            Assert.Single(page3);
        }

        [Fact]
        public async Task GetPagedAsync_SortByTitleAscendingAndDescending()
        {
            // Arrange
            var project = new Project("Sort Project", ProjectPriority.Medium);
            var projectRepo = new ProjectRepository(CreateContext());
            await projectRepo.AddAsync(project);

            var taskRepo = new TaskRepository(CreateContext());
            await taskRepo.AddAsync(new DevTask(project.Id, "Task C", TaskPriority.Medium));
            await taskRepo.AddAsync(new DevTask(project.Id, "Task A", TaskPriority.Medium));
            await taskRepo.AddAsync(new DevTask(project.Id, "Task B", TaskPriority.Medium));

            // Act
            var ascResult = await taskRepo.GetPagedAsync(project.Id, 1, 10, null, null, null, "Title", "asc");
            var descResult = await taskRepo.GetPagedAsync(project.Id, 1, 10, null, null, null, "Title", "desc");

            // Assert
            Assert.Equal("Task A", ascResult[0].Title);
            Assert.Equal("Task B", ascResult[1].Title);
            Assert.Equal("Task C", ascResult[2].Title);

            Assert.Equal("Task C", descResult[0].Title);
            Assert.Equal("Task B", descResult[1].Title);
            Assert.Equal("Task A", descResult[2].Title);
        }

        [Fact]
        public async Task DeletingProject_CascadesAndDeletesTasks()
        {
            // Arrange
            var project = new Project("Cascade Project", ProjectPriority.Medium);
            var projectRepo = new ProjectRepository(CreateContext());
            await projectRepo.AddAsync(project);

            var task1 = new DevTask(project.Id, "Task 1", TaskPriority.Medium);
            var task2 = new DevTask(project.Id, "Task 2", TaskPriority.High);
            var taskRepo = new TaskRepository(CreateContext());
            await taskRepo.AddAsync(task1);
            await taskRepo.AddAsync(task2);

            // Act
            await projectRepo.DeleteAsync(project);

            // Assert
            using var verifyContext = CreateContext();
            var remainingTasks = await verifyContext.DevTasks
                .AsNoTracking()
                .Where(t => t.ProjectId == project.Id)
                .ToListAsync();

            Assert.Empty(remainingTasks);
        }
    }
}
