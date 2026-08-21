using DevOS.Domain.Entities;
using DevOS.Infrastructure.Persistence;
using DevOS.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace DevOS.Infrastructure.Tests
{
    public class TimeEntryRepositoryTests : IAsyncLifetime
    {
        private readonly PostgreSqlContainer _postgres;

        public TimeEntryRepositoryTests()
        {
            _postgres = new PostgreSqlBuilder("postgres:16")
                .WithDatabase("devos_timeentry_test")
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
        public async Task AddAsync_ValidTimeEntry_PersistsTimeEntry()
        {
            // Arrange
            var project = new Project("TimeEntry Project", ProjectPriority.Medium);
            var projectRepository = new ProjectRepository(CreateContext());
            await projectRepository.AddAsync(project);

            var startedAt = new DateTime(2026, 8, 21, 10, 0, 0, DateTimeKind.Utc);
            var endedAt = new DateTime(2026, 8, 21, 11, 30, 0, DateTimeKind.Utc);

            var timeEntry = new TimeEntry(
                project.Id,
                startedAt,
                endedAt,
                "Coding session");

            var repository = new TimeEntryRepository(CreateContext());

            // Act
            await repository.AddAsync(timeEntry);

            // Assert
            using var verifyContext = CreateContext();
            var persisted = await verifyContext.TimeEntries
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == timeEntry.Id);

            Assert.NotNull(persisted);
            Assert.Equal(timeEntry.Id, persisted.Id);
            Assert.Equal(project.Id, persisted.ProjectId);
            Assert.Null(persisted.TaskId);
            Assert.True(Math.Abs((timeEntry.StartedAt - persisted.StartedAt).TotalMilliseconds) < 1);
            Assert.True(Math.Abs((timeEntry.EndedAt - persisted.EndedAt).TotalMilliseconds) < 1);
            Assert.Equal(90, persisted.DurationMinutes);
            Assert.Equal("Coding session", persisted.Description);
            Assert.True(Math.Abs((timeEntry.CreatedAt - persisted.CreatedAt).TotalMilliseconds) < 1);
            Assert.True(Math.Abs((timeEntry.UpdatedAt - persisted.UpdatedAt).TotalMilliseconds) < 1);
        }

        [Fact]
        public async Task GetByIdAsync_ExistingTimeEntry_ReturnsTimeEntry()
        {
            // Arrange
            var project = new Project("GetById Project", ProjectPriority.Medium);
            var projectRepository = new ProjectRepository(CreateContext());
            await projectRepository.AddAsync(project);

            var startedAt = new DateTime(2026, 8, 21, 10, 0, 0, DateTimeKind.Utc);
            var endedAt = new DateTime(2026, 8, 21, 11, 0, 0, DateTimeKind.Utc);

            var timeEntry = new TimeEntry(project.Id, startedAt, endedAt, "Test entry");
            var addRepository = new TimeEntryRepository(CreateContext());
            await addRepository.AddAsync(timeEntry);

            var repository = new TimeEntryRepository(CreateContext());

            // Act
            var result = await repository.GetByIdAsync(timeEntry.Id, project.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(timeEntry.Id, result.Id);
            Assert.Equal(project.Id, result.ProjectId);
            Assert.Null(result.TaskId);
            Assert.True(Math.Abs((timeEntry.StartedAt - result.StartedAt).TotalMilliseconds) < 1);
            Assert.True(Math.Abs((timeEntry.EndedAt - result.EndedAt).TotalMilliseconds) < 1);
            Assert.Equal(60, result.DurationMinutes);
            Assert.Equal("Test entry", result.Description);
        }

        [Fact]
        public async Task GetByIdAsync_TimeEntryBelongsToAnotherProject_ReturnsNull()
        {
            // Arrange
            var projectA = new Project("Project A", ProjectPriority.Medium);
            var projectB = new Project("Project B", ProjectPriority.Medium);

            var projectRepository = new ProjectRepository(CreateContext());
            await projectRepository.AddAsync(projectA);
            await projectRepository.AddAsync(projectB);

            var timeEntry = new TimeEntry(
                projectA.Id,
                new DateTime(2026, 8, 21, 10, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 8, 21, 11, 0, 0, DateTimeKind.Utc),
                "Entry for Project A");

            var addRepository = new TimeEntryRepository(CreateContext());
            await addRepository.AddAsync(timeEntry);

            var repository = new TimeEntryRepository(CreateContext());

            // Act
            var result = await repository.GetByIdAsync(timeEntry.Id, projectB.Id);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistingId_ReturnsNull()
        {
            // Arrange
            var project = new Project("NonExisting Project", ProjectPriority.Medium);
            var projectRepository = new ProjectRepository(CreateContext());
            await projectRepository.AddAsync(project);

            var repository = new TimeEntryRepository(CreateContext());

            // Act
            var result = await repository.GetByIdAsync(Guid.NewGuid(), project.Id);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllByProjectIdAsync_ReturnsOnlyProjectEntries()
        {
            // Arrange
            var projectA = new Project("Project A", ProjectPriority.Medium);
            var projectB = new Project("Project B", ProjectPriority.Medium);

            var projectRepository = new ProjectRepository(CreateContext());
            await projectRepository.AddAsync(projectA);
            await projectRepository.AddAsync(projectB);

            var entryA1 = new TimeEntry(projectA.Id,
                new DateTime(2026, 8, 21, 9, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 8, 21, 10, 0, 0, DateTimeKind.Utc), "A1");

            var entryA2 = new TimeEntry(projectA.Id,
                new DateTime(2026, 8, 21, 11, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 8, 21, 12, 0, 0, DateTimeKind.Utc), "A2");

            var entryB1 = new TimeEntry(projectB.Id,
                new DateTime(2026, 8, 21, 13, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 8, 21, 14, 0, 0, DateTimeKind.Utc), "B1");

            var addRepository = new TimeEntryRepository(CreateContext());
            await addRepository.AddAsync(entryA1);
            await addRepository.AddAsync(entryA2);
            await addRepository.AddAsync(entryB1);

            var repository = new TimeEntryRepository(CreateContext());

            // Act
            var result = await repository.GetAllByProjectIdAsync(projectA.Id);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, t => t.Id == entryA1.Id);
            Assert.Contains(result, t => t.Id == entryA2.Id);
            Assert.DoesNotContain(result, t => t.Id == entryB1.Id);
        }

        [Fact]
        public async Task GetAllByProjectIdAsync_ReturnsEntriesOrderedByStartedAt()
        {
            // Arrange
            var project = new Project("Ordered Project", ProjectPriority.Medium);
            var projectRepository = new ProjectRepository(CreateContext());
            await projectRepository.AddAsync(project);

            var entry1 = new TimeEntry(project.Id,
                new DateTime(2026, 8, 21, 14, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 8, 21, 15, 0, 0, DateTimeKind.Utc), "14:00");

            var entry2 = new TimeEntry(project.Id,
                new DateTime(2026, 8, 21, 9, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 8, 21, 10, 0, 0, DateTimeKind.Utc), "09:00");

            var entry3 = new TimeEntry(project.Id,
                new DateTime(2026, 8, 21, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 8, 21, 13, 0, 0, DateTimeKind.Utc), "12:00");

            var addRepository = new TimeEntryRepository(CreateContext());
            await addRepository.AddAsync(entry1);
            await addRepository.AddAsync(entry2);
            await addRepository.AddAsync(entry3);

            var repository = new TimeEntryRepository(CreateContext());

            // Act
            var result = await repository.GetAllByProjectIdAsync(project.Id);

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal(entry2.Id, result[0].Id);
            Assert.Equal(entry3.Id, result[1].Id);
            Assert.Equal(entry1.Id, result[2].Id);
        }

        [Fact]
        public async Task GetAllByTaskIdAsync_ReturnsOnlyTaskEntries()
        {
            // Arrange
            var project = new Project("Task Project", ProjectPriority.Medium);
            var projectRepository = new ProjectRepository(CreateContext());
            await projectRepository.AddAsync(project);

            var taskA = new DevTask(project.Id, "Task A", TaskPriority.Medium);
            var taskB = new DevTask(project.Id, "Task B", TaskPriority.Medium);

            var taskRepository = new TaskRepository(CreateContext());
            await taskRepository.AddAsync(taskA);
            await taskRepository.AddAsync(taskB);

            var entryA1 = new TimeEntry(project.Id,
                new DateTime(2026, 8, 21, 9, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 8, 21, 10, 0, 0, DateTimeKind.Utc),
                "A1", taskA.Id);

            var entryA2 = new TimeEntry(project.Id,
                new DateTime(2026, 8, 21, 11, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 8, 21, 12, 0, 0, DateTimeKind.Utc),
                "A2", taskA.Id);

            var entryB1 = new TimeEntry(project.Id,
                new DateTime(2026, 8, 21, 13, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 8, 21, 14, 0, 0, DateTimeKind.Utc),
                "B1", taskB.Id);

            var entryNoTask = new TimeEntry(project.Id,
                new DateTime(2026, 8, 21, 15, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 8, 21, 16, 0, 0, DateTimeKind.Utc),
                "No Task");

            var addRepository = new TimeEntryRepository(CreateContext());
            await addRepository.AddAsync(entryA1);
            await addRepository.AddAsync(entryA2);
            await addRepository.AddAsync(entryB1);
            await addRepository.AddAsync(entryNoTask);

            var repository = new TimeEntryRepository(CreateContext());

            // Act
            var result = await repository.GetAllByTaskIdAsync(taskA.Id);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, t => t.Id == entryA1.Id);
            Assert.Contains(result, t => t.Id == entryA2.Id);
            Assert.DoesNotContain(result, t => t.Id == entryB1.Id);
            Assert.DoesNotContain(result, t => t.Id == entryNoTask.Id);
        }

        [Fact]
        public async Task GetAllByTaskIdAsync_ReturnsEntriesOrderedByStartedAt()
        {
            // Arrange
            var project = new Project("Task Ordered Project", ProjectPriority.Medium);
            var projectRepository = new ProjectRepository(CreateContext());
            await projectRepository.AddAsync(project);

            var task = new DevTask(project.Id, "Ordered Task", TaskPriority.Medium);
            var taskRepository = new TaskRepository(CreateContext());
            await taskRepository.AddAsync(task);

            var entry1 = new TimeEntry(project.Id,
                new DateTime(2026, 8, 21, 16, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 8, 21, 17, 0, 0, DateTimeKind.Utc),
                "16:00", task.Id);

            var entry2 = new TimeEntry(project.Id,
                new DateTime(2026, 8, 21, 11, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 8, 21, 12, 0, 0, DateTimeKind.Utc),
                "11:00", task.Id);

            var entry3 = new TimeEntry(project.Id,
                new DateTime(2026, 8, 21, 13, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 8, 21, 14, 0, 0, DateTimeKind.Utc),
                "13:00", task.Id);

            var addRepository = new TimeEntryRepository(CreateContext());
            await addRepository.AddAsync(entry1);
            await addRepository.AddAsync(entry2);
            await addRepository.AddAsync(entry3);

            var repository = new TimeEntryRepository(CreateContext());

            // Act
            var result = await repository.GetAllByTaskIdAsync(task.Id);

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal(entry2.Id, result[0].Id);
            Assert.Equal(entry3.Id, result[1].Id);
            Assert.Equal(entry1.Id, result[2].Id);
        }

        [Fact]
        public async Task GetByPeriodAsync_ReturnsEntriesWithinPeriod()
        {
            // Arrange
            var project = new Project("Period Project", ProjectPriority.Medium);
            var projectRepository = new ProjectRepository(CreateContext());
            await projectRepository.AddAsync(project);

            var entry1 = new TimeEntry(project.Id,
                new DateTime(2026, 8, 21, 9, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 8, 21, 10, 0, 0, DateTimeKind.Utc), "09:00");

            var entry2 = new TimeEntry(project.Id,
                new DateTime(2026, 8, 21, 11, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 8, 21, 12, 0, 0, DateTimeKind.Utc), "11:00");

            var entry3 = new TimeEntry(project.Id,
                new DateTime(2026, 8, 21, 14, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 8, 21, 15, 0, 0, DateTimeKind.Utc), "14:00");

            var entry4 = new TimeEntry(project.Id,
                new DateTime(2026, 8, 21, 17, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 8, 21, 18, 0, 0, DateTimeKind.Utc), "17:00");

            var addRepository = new TimeEntryRepository(CreateContext());
            await addRepository.AddAsync(entry1);
            await addRepository.AddAsync(entry2);
            await addRepository.AddAsync(entry3);
            await addRepository.AddAsync(entry4);

            var repository = new TimeEntryRepository(CreateContext());

            var from = new DateTime(2026, 8, 21, 10, 0, 0, DateTimeKind.Utc);
            var to = new DateTime(2026, 8, 21, 16, 0, 0, DateTimeKind.Utc);

            // Act
            var result = await repository.GetByPeriodAsync(project.Id, from, to);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, t => t.Id == entry2.Id);
            Assert.Contains(result, t => t.Id == entry3.Id);
        }

        [Fact]
        public async Task GetByPeriodAsync_UsesInclusiveFromAndExclusiveTo()
        {
            // Arrange
            var project = new Project("Boundary Project", ProjectPriority.Medium);
            var projectRepository = new ProjectRepository(CreateContext());
            await projectRepository.AddAsync(project);

            var entryAtFrom = new TimeEntry(project.Id,
                new DateTime(2026, 8, 21, 10, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 8, 21, 11, 0, 0, DateTimeKind.Utc), "At From");

            var entryAtTo = new TimeEntry(project.Id,
                new DateTime(2026, 8, 21, 16, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 8, 21, 17, 0, 0, DateTimeKind.Utc), "At To");

            var addRepository = new TimeEntryRepository(CreateContext());
            await addRepository.AddAsync(entryAtFrom);
            await addRepository.AddAsync(entryAtTo);

            var repository = new TimeEntryRepository(CreateContext());

            var from = new DateTime(2026, 8, 21, 10, 0, 0, DateTimeKind.Utc);
            var to = new DateTime(2026, 8, 21, 16, 0, 0, DateTimeKind.Utc);

            // Act
            var result = await repository.GetByPeriodAsync(project.Id, from, to);

            // Assert
            Assert.Single(result);
            Assert.Equal(entryAtFrom.Id, result[0].Id);
        }

        [Fact]
        public async Task GetByPeriodAsync_ReturnsOnlyEntriesFromProject()
        {
            // Arrange
            var projectA = new Project("Period Project A", ProjectPriority.Medium);
            var projectB = new Project("Period Project B", ProjectPriority.Medium);

            var projectRepository = new ProjectRepository(CreateContext());
            await projectRepository.AddAsync(projectA);
            await projectRepository.AddAsync(projectB);

            var entryA = new TimeEntry(projectA.Id,
                new DateTime(2026, 8, 21, 10, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 8, 21, 11, 0, 0, DateTimeKind.Utc), "Entry A");

            var entryB = new TimeEntry(projectB.Id,
                new DateTime(2026, 8, 21, 10, 30, 0, DateTimeKind.Utc),
                new DateTime(2026, 8, 21, 11, 30, 0, DateTimeKind.Utc), "Entry B");

            var addRepository = new TimeEntryRepository(CreateContext());
            await addRepository.AddAsync(entryA);
            await addRepository.AddAsync(entryB);

            var repository = new TimeEntryRepository(CreateContext());

            var from = new DateTime(2026, 8, 21, 9, 0, 0, DateTimeKind.Utc);
            var to = new DateTime(2026, 8, 21, 12, 0, 0, DateTimeKind.Utc);

            // Act
            var result = await repository.GetByPeriodAsync(projectA.Id, from, to);

            // Assert
            Assert.Single(result);
            Assert.Equal(entryA.Id, result[0].Id);
        }

        [Fact]
        public async Task UpdateAsync_ExistingTimeEntry_UpdatesTimeEntry()
        {
            // Arrange
            var project = new Project("Update TimeEntry Project", ProjectPriority.Medium);
            var projectRepository = new ProjectRepository(CreateContext());
            await projectRepository.AddAsync(project);

            var timeEntry = new TimeEntry(project.Id,
                new DateTime(2026, 8, 21, 10, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 8, 21, 11, 0, 0, DateTimeKind.Utc),
                "Coding");

            var addRepository = new TimeEntryRepository(CreateContext());
            await addRepository.AddAsync(timeEntry);

            var originalUpdatedAt = timeEntry.UpdatedAt;

            timeEntry.UpdateTimeRange(
                new DateTime(2026, 8, 21, 10, 30, 0, DateTimeKind.Utc),
                new DateTime(2026, 8, 21, 12, 0, 0, DateTimeKind.Utc));

            timeEntry.UpdateDescription("Architecture");

            var repository = new TimeEntryRepository(CreateContext());

            // Act
            await repository.UpdateAsync(timeEntry);

            // Assert
            using var verifyContext = CreateContext();
            var updated = await verifyContext.TimeEntries
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == timeEntry.Id);

            Assert.NotNull(updated);
            Assert.Equal(90, updated.DurationMinutes);
            Assert.Equal("Architecture", updated.Description);
            Assert.True(Math.Abs((timeEntry.StartedAt - updated.StartedAt).TotalMilliseconds) < 1);
            Assert.True(Math.Abs((timeEntry.EndedAt - updated.EndedAt).TotalMilliseconds) < 1);
            Assert.True(updated.UpdatedAt >= originalUpdatedAt);
        }

        [Fact]
        public async Task DeleteAsync_ExistingTimeEntry_RemovesTimeEntry()
        {
            // Arrange
            var project = new Project("Delete TimeEntry Project", ProjectPriority.Medium);
            var projectRepository = new ProjectRepository(CreateContext());
            await projectRepository.AddAsync(project);

            var timeEntry = new TimeEntry(project.Id,
                new DateTime(2026, 8, 21, 10, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 8, 21, 11, 0, 0, DateTimeKind.Utc),
                "To Delete");

            var addRepository = new TimeEntryRepository(CreateContext());
            await addRepository.AddAsync(timeEntry);

            var repository = new TimeEntryRepository(CreateContext());

            // Act
            await repository.DeleteAsync(timeEntry);

            // Assert
            using var verifyContext = CreateContext();
            var deleted = await verifyContext.TimeEntries
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == timeEntry.Id);

            Assert.Null(deleted);
        }

        [Fact]
        public async Task DeletingProject_DeletesItsTimeEntries()
        {
            // Arrange
            var project = new Project("Cascade Delete Project", ProjectPriority.Medium);
            var projectRepository = new ProjectRepository(CreateContext());
            await projectRepository.AddAsync(project);

            var entry1 = new TimeEntry(project.Id,
                new DateTime(2026, 8, 21, 9, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 8, 21, 10, 0, 0, DateTimeKind.Utc), "Entry 1");

            var entry2 = new TimeEntry(project.Id,
                new DateTime(2026, 8, 21, 11, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 8, 21, 12, 0, 0, DateTimeKind.Utc), "Entry 2");

            var timeEntryRepository = new TimeEntryRepository(CreateContext());
            await timeEntryRepository.AddAsync(entry1);
            await timeEntryRepository.AddAsync(entry2);

            var deleteRepository = new ProjectRepository(CreateContext());

            // Act
            await deleteRepository.DeleteAsync(project);

            // Assert
            using var verifyContext = CreateContext();
            var entries = await verifyContext.TimeEntries
                .AsNoTracking()
                .Where(t => t.ProjectId == project.Id)
                .ToListAsync();

            Assert.Empty(entries);
        }

        [Fact]
        public async Task DeletingTask_SetsTaskIdToNullAndKeepsTimeEntry()
        {
            // Arrange
            var project = new Project("SetNull Project", ProjectPriority.Medium);
            var projectRepository = new ProjectRepository(CreateContext());
            await projectRepository.AddAsync(project);

            var task = new DevTask(project.Id, "Task to Delete", TaskPriority.Medium);
            var taskRepository = new TaskRepository(CreateContext());
            await taskRepository.AddAsync(task);

            var timeEntry = new TimeEntry(project.Id,
                new DateTime(2026, 8, 21, 10, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 8, 21, 11, 0, 0, DateTimeKind.Utc),
                "Entry with Task", task.Id);

            var timeEntryRepository = new TimeEntryRepository(CreateContext());
            await timeEntryRepository.AddAsync(timeEntry);

            var deleteTaskRepository = new TaskRepository(CreateContext());

            // Act
            await deleteTaskRepository.DeleteAsync(task);

            // Assert
            using var verifyContext = CreateContext();
            var persistedEntry = await verifyContext.TimeEntries
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == timeEntry.Id);

            Assert.NotNull(persistedEntry);
            Assert.Null(persistedEntry.TaskId);
            Assert.Equal(project.Id, persistedEntry.ProjectId);
            Assert.Equal(60, persistedEntry.DurationMinutes);
            Assert.Equal("Entry with Task", persistedEntry.Description);
            Assert.True(Math.Abs((timeEntry.StartedAt - persistedEntry.StartedAt).TotalMilliseconds) < 1);
            Assert.True(Math.Abs((timeEntry.EndedAt - persistedEntry.EndedAt).TotalMilliseconds) < 1);
        }
    }
}