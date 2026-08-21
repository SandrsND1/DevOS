using DevOS.Domain.Entities;
using DevOS.Infrastructure.Persistence;
using DevOS.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace DevOS.Infrastructure.Tests
{
    public class ProjectRepositoryTests : IAsyncLifetime
    {
        private readonly PostgreSqlContainer _postgres;

        public ProjectRepositoryTests()
        {
            _postgres = new PostgreSqlBuilder("postgres:16")
                .WithDatabase("devos_project_test")
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
        public async Task AddAsync_ValidProject_PersistsProject()
        {
            // Arrange
            var project = new Project(
                "Integration Test Project",
                ProjectPriority.High,
                "Integration test description",
                DateTime.UtcNow.AddDays(30));

            var repository = new ProjectRepository(CreateContext());

            // Act
            await repository.AddAsync(project);

            // Assert
            using var verifyContext = CreateContext();
            var persisted = await verifyContext.Projects
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == project.Id);

            Assert.NotNull(persisted);
            Assert.Equal(project.Id, persisted.Id);
            Assert.Equal(project.Name, persisted.Name);
            Assert.Equal(project.Priority, persisted.Priority);
            Assert.Equal(project.Status, persisted.Status);
            Assert.True(Math.Abs((project.Deadline!.Value - persisted.Deadline!.Value).TotalMilliseconds) < 1);
            Assert.True(Math.Abs((project.CreatedAt - persisted.CreatedAt).TotalMilliseconds) < 1);
            Assert.True(Math.Abs((project.UpdatedAt - persisted.UpdatedAt).TotalMilliseconds) < 1);
        }

        [Fact]
        public async Task GetByIdAsync_ExistingProject_ReturnsProject()
        {
            // Arrange
            var project = new Project(
                "GetById Test Project",
                ProjectPriority.Medium,
                "GetById test description",
                DateTime.UtcNow.AddDays(15));

            var addRepository = new ProjectRepository(CreateContext());
            await addRepository.AddAsync(project);

            var repository = new ProjectRepository(CreateContext());

            // Act
            var result = await repository.GetByIdAsync(project.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(project.Id, result.Id);
            Assert.Equal(project.Name, result.Name);
            Assert.Equal(project.Priority, result.Priority);
            Assert.Equal(project.Status, result.Status);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistingId_ReturnsNull()
        {
            // Arrange
            var repository = new ProjectRepository(CreateContext());

            // Act
            var result = await repository.GetByIdAsync(Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllProjects()
        {
            // Arrange
            var project1 = new Project("Project Alpha", ProjectPriority.Low, "Alpha description");
            var project2 = new Project("Project Beta", ProjectPriority.High, "Beta description");
            var project3 = new Project("Project Gamma", ProjectPriority.Critical, "Gamma description");

            var addRepository = new ProjectRepository(CreateContext());
            await addRepository.AddAsync(project1);
            await addRepository.AddAsync(project2);
            await addRepository.AddAsync(project3);

            var repository = new ProjectRepository(CreateContext());

            // Act
            var result = await repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Contains(result, p => p.Id == project1.Id && p.Name == project1.Name);
            Assert.Contains(result, p => p.Id == project2.Id && p.Name == project2.Name);
            Assert.Contains(result, p => p.Id == project3.Id && p.Name == project3.Name);
        }

        [Fact]
        public async Task UpdateAsync_ExistingProject_UpdatesProject()
        {
            // Arrange
            var project = new Project(
                "Original Project",
                ProjectPriority.Medium,
                "Original description",
                DateTime.UtcNow.AddDays(10));

            var addRepository = new ProjectRepository(CreateContext());
            await addRepository.AddAsync(project);

            var originalUpdatedAt = project.UpdatedAt;

            project.UpdateName("Updated Project");
            project.UpdateDescription("Updated description");
            project.UpdatePriority(ProjectPriority.High);
            project.UpdateDeadline(DateTime.UtcNow.AddDays(25));

            var repository = new ProjectRepository(CreateContext());

            // Act
            await repository.UpdateAsync(project);

            // Assert
            using var verifyContext = CreateContext();
            var updated = await verifyContext.Projects
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == project.Id);

            Assert.NotNull(updated);
            Assert.Equal("Updated Project", updated.Name);
            Assert.Equal("Updated description", updated.Description);
            Assert.Equal(ProjectPriority.High, updated.Priority);
            Assert.True(updated.UpdatedAt >= originalUpdatedAt);
        }

        [Fact]
        public async Task DeleteAsync_ExistingProject_RemovesProject()
        {
            // Arrange
            var project = new Project("Delete Test Project", ProjectPriority.Medium);
            var addRepository = new ProjectRepository(CreateContext());
            await addRepository.AddAsync(project);

            var repository = new ProjectRepository(CreateContext());

            // Act
            await repository.DeleteAsync(project);

            // Assert
            using var verifyContext = CreateContext();
            var deleted = await verifyContext.Projects
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == project.Id);

            Assert.Null(deleted);
        }

        [Fact]
        public async Task DeleteAsync_ProjectWithTasksAndTimeEntries_RemovesProjectAndCascades()
        {
            // Arrange
            var project = new Project("Cascade Project", ProjectPriority.Medium);
            var projectRepository = new ProjectRepository(CreateContext());
            await projectRepository.AddAsync(project);

            var task1 = new DevTask(project.Id, "Task 1", TaskPriority.Medium);
            var task2 = new DevTask(project.Id, "Task 2", TaskPriority.High);
            var taskRepository = new TaskRepository(CreateContext());
            await taskRepository.AddAsync(task1);
            await taskRepository.AddAsync(task2);

            var entry1 = new TimeEntry(project.Id,
                new DateTime(2026, 8, 21, 9, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 8, 21, 10, 0, 0, DateTimeKind.Utc),
                "Entry 1", task1.Id);

            var entry2 = new TimeEntry(project.Id,
                new DateTime(2026, 8, 21, 11, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 8, 21, 12, 0, 0, DateTimeKind.Utc),
                "Entry 2", task2.Id);

            var entryNoTask = new TimeEntry(project.Id,
                new DateTime(2026, 8, 21, 13, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 8, 21, 14, 0, 0, DateTimeKind.Utc),
                "Entry without task");

            var timeEntryRepository = new TimeEntryRepository(CreateContext());
            await timeEntryRepository.AddAsync(entry1);
            await timeEntryRepository.AddAsync(entry2);
            await timeEntryRepository.AddAsync(entryNoTask);

            var deleteRepository = new ProjectRepository(CreateContext());

            // Act
            await deleteRepository.DeleteAsync(project);

            // Assert
            using var verifyContext = CreateContext();

            var deletedProject = await verifyContext.Projects
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == project.Id);
            Assert.Null(deletedProject);

            var tasks = await verifyContext.DevTasks
                .AsNoTracking()
                .Where(t => t.ProjectId == project.Id)
                .ToListAsync();
            Assert.Empty(tasks);

            var timeEntries = await verifyContext.TimeEntries
                .AsNoTracking()
                .Where(t => t.ProjectId == project.Id)
                .ToListAsync();
            Assert.Empty(timeEntries);
        }

        [Fact]
        public async Task GetPagedAsync_ReturnsPaginatedProjects()
        {
            // Arrange
            for (int i = 1; i <= 5; i++)
            {
                var project = new Project($"Paged Project {i}", ProjectPriority.Medium);
                var addRepository = new ProjectRepository(CreateContext());
                await addRepository.AddAsync(project);
                await Task.Delay(10); // Ensure different CreatedAt
            }

            var repository = new ProjectRepository(CreateContext());

            // Act
            var page1 = await repository.GetPagedAsync(1, 2, null, null, null, "CreatedAt", "desc");
            var page2 = await repository.GetPagedAsync(2, 2, null, null, null, "CreatedAt", "desc");
            var page3 = await repository.GetPagedAsync(3, 2, null, null, null, "CreatedAt", "desc");

            // Assert
            Assert.Equal(2, page1.Count);
            Assert.Equal(2, page2.Count);
            Assert.Single(page3);
        }

        [Fact]
        public async Task GetTotalCountAsync_ReturnsFilteredCount()
        {
            // Arrange
            var project1 = new Project("Active Project", ProjectPriority.High);
            project1.UpdateStatus(ProjectStatus.Active);
            var project2 = new Project("Planning Project", ProjectPriority.Medium);
            var project3 = new Project("Active Low Project", ProjectPriority.Low);
            project3.UpdateStatus(ProjectStatus.Active);

            var addRepository = new ProjectRepository(CreateContext());
            await addRepository.AddAsync(project1);
            await addRepository.AddAsync(project2);
            await addRepository.AddAsync(project3);

            var repository = new ProjectRepository(CreateContext());

            // Act
            var activeCount = await repository.GetTotalCountAsync(ProjectStatus.Active, null, null);
            var highCount = await repository.GetTotalCountAsync(null, ProjectPriority.High, null);
            var activeHighCount = await repository.GetTotalCountAsync(ProjectStatus.Active, ProjectPriority.High, null);

            // Assert
            Assert.Equal(2, activeCount);
            Assert.Equal(1, highCount);
            Assert.Equal(1, activeHighCount);
        }
        [Fact]
public async Task GetPagedAsync_FilterByStatus_ReturnsOnlyMatchingProjects()
{
    // Arrange
    var project1 = new Project("Active 1", ProjectPriority.Medium);
    project1.UpdateStatus(ProjectStatus.Active);
    var project2 = new Project("Active 2", ProjectPriority.Medium);
    project2.UpdateStatus(ProjectStatus.Active);
    var project3 = new Project("Planning", ProjectPriority.Medium);
    var project4 = new Project("Completed", ProjectPriority.Medium);
    project4.UpdateStatus(ProjectStatus.Completed);

    var addRepository = new ProjectRepository(CreateContext());
    await addRepository.AddAsync(project1);
    await addRepository.AddAsync(project2);
    await addRepository.AddAsync(project3);
    await addRepository.AddAsync(project4);

    var repository = new ProjectRepository(CreateContext());

    // Act
    var result = await repository.GetPagedAsync(
        1, 10, ProjectStatus.Active, null, null, "CreatedAt", "desc");

    // Assert
    Assert.Equal(2, result.Count);
    Assert.All(result, p => Assert.Equal(ProjectStatus.Active, p.Status));
}

[Fact]
public async Task GetPagedAsync_FilterByPriority_ReturnsOnlyMatchingProjects()
{
    // Arrange
    var project1 = new Project("Low", ProjectPriority.Low);
    var project2 = new Project("Medium", ProjectPriority.Medium);
    var project3 = new Project("High 1", ProjectPriority.High);
    var project4 = new Project("Critical", ProjectPriority.Critical);
    var project5 = new Project("High 2", ProjectPriority.High);

    var addRepository = new ProjectRepository(CreateContext());
    await addRepository.AddAsync(project1);
    await addRepository.AddAsync(project2);
    await addRepository.AddAsync(project3);
    await addRepository.AddAsync(project4);
    await addRepository.AddAsync(project5);

    var repository = new ProjectRepository(CreateContext());

    // Act
    var result = await repository.GetPagedAsync(
        1, 10, null, ProjectPriority.High, null, "CreatedAt", "desc");

    // Assert
    Assert.Equal(2, result.Count);
    Assert.All(result, p => Assert.Equal(ProjectPriority.High, p.Priority));
}

[Fact]
public async Task GetPagedAsync_FilterByStatusAndPriority_ReturnsMatchingProjects()
{
    // Arrange
    var project1 = new Project("Active High", ProjectPriority.High);
    project1.UpdateStatus(ProjectStatus.Active);

    var project2 = new Project("Active Medium", ProjectPriority.Medium);
    project2.UpdateStatus(ProjectStatus.Active);

    var project3 = new Project("Planning High", ProjectPriority.High);

    var project4 = new Project("Completed High", ProjectPriority.High);
    project4.UpdateStatus(ProjectStatus.Completed);

    var addRepository = new ProjectRepository(CreateContext());
    await addRepository.AddAsync(project1);
    await addRepository.AddAsync(project2);
    await addRepository.AddAsync(project3);
    await addRepository.AddAsync(project4);

    var repository = new ProjectRepository(CreateContext());

    // Act
    var result = await repository.GetPagedAsync(
        1, 10, ProjectStatus.Active, ProjectPriority.High, null, "CreatedAt", "desc");

    // Assert
    Assert.Single(result);
    Assert.Equal(ProjectStatus.Active, result[0].Status);
    Assert.Equal(ProjectPriority.High, result[0].Priority);
}

[Fact]
public async Task GetPagedAsync_SearchByName_ReturnsMatchingProjects()
{
    // Arrange
    var project1 = new Project("DevOS Backend", ProjectPriority.Medium);
    var project2 = new Project("DevOS Frontend", ProjectPriority.Medium);
    var project3 = new Project("Game Engine", ProjectPriority.Medium);
    var project4 = new Project("Mobile Application", ProjectPriority.Medium);

    var addRepository = new ProjectRepository(CreateContext());
    await addRepository.AddAsync(project1);
    await addRepository.AddAsync(project2);
    await addRepository.AddAsync(project3);
    await addRepository.AddAsync(project4);

    var repository = new ProjectRepository(CreateContext());

    // Act
    var result = await repository.GetPagedAsync(
        1, 10, null, null, "DevOS", "CreatedAt", "desc");

    // Assert
    Assert.Equal(2, result.Count);
    Assert.All(result, p => Assert.Contains("DevOS", p.Name));
}

[Fact]
public async Task GetPagedAsync_SearchNoResults_ReturnsEmptyList()
{
    // Arrange
    var project1 = new Project("DevOS Backend", ProjectPriority.Medium);
    var addRepository = new ProjectRepository(CreateContext());
    await addRepository.AddAsync(project1);

    var repository = new ProjectRepository(CreateContext());

    // Act
    var result = await repository.GetPagedAsync(
        1, 10, null, null, "SomethingThatDoesNotExist", "CreatedAt", "desc");

    // Assert
    Assert.Empty(result);
}

[Fact]
public async Task GetPagedAsync_SortByNameAscending_ReturnsOrderedProjects()
{
    // Arrange
    var project1 = new Project("Zeta", ProjectPriority.Medium);
    var project2 = new Project("Alpha", ProjectPriority.Medium);
    var project3 = new Project("Gamma", ProjectPriority.Medium);
    var project4 = new Project("Beta", ProjectPriority.Medium);

    var addRepository = new ProjectRepository(CreateContext());
    await addRepository.AddAsync(project1);
    await addRepository.AddAsync(project2);
    await addRepository.AddAsync(project3);
    await addRepository.AddAsync(project4);

    var repository = new ProjectRepository(CreateContext());

    // Act
    var result = await repository.GetPagedAsync(
        1, 10, null, null, null, "Name", "asc");

    // Assert
    Assert.Equal(4, result.Count);
    Assert.Equal("Alpha", result[0].Name);
    Assert.Equal("Beta", result[1].Name);
    Assert.Equal("Gamma", result[2].Name);
    Assert.Equal("Zeta", result[3].Name);
}

[Fact]
public async Task GetPagedAsync_SortByNameDescending_ReturnsOrderedProjects()
{
    // Arrange
    var project1 = new Project("Zeta", ProjectPriority.Medium);
    var project2 = new Project("Alpha", ProjectPriority.Medium);
    var project3 = new Project("Gamma", ProjectPriority.Medium);
    var project4 = new Project("Beta", ProjectPriority.Medium);

    var addRepository = new ProjectRepository(CreateContext());
    await addRepository.AddAsync(project1);
    await addRepository.AddAsync(project2);
    await addRepository.AddAsync(project3);
    await addRepository.AddAsync(project4);

    var repository = new ProjectRepository(CreateContext());

    // Act
    var result = await repository.GetPagedAsync(
        1, 10, null, null, null, "Name", "desc");

    // Assert
    Assert.Equal(4, result.Count);
    Assert.Equal("Zeta", result[0].Name);
    Assert.Equal("Gamma", result[1].Name);
    Assert.Equal("Beta", result[2].Name);
    Assert.Equal("Alpha", result[3].Name);
}

[Fact]
public async Task GetPagedAsync_SortByPriority_ReturnsProjectsInEnumOrder()
{
    // Arrange
    var project1 = new Project("Critical", ProjectPriority.Critical);
    var project2 = new Project("Low", ProjectPriority.Low);
    var project3 = new Project("High", ProjectPriority.High);
    var project4 = new Project("Medium", ProjectPriority.Medium);

    var addRepository = new ProjectRepository(CreateContext());
    await addRepository.AddAsync(project1);
    await addRepository.AddAsync(project2);
    await addRepository.AddAsync(project3);
    await addRepository.AddAsync(project4);

    var repository = new ProjectRepository(CreateContext());

    // Act
    var resultAsc = await repository.GetPagedAsync(
        1, 10, null, null, null, "Priority", "asc");

    // Assert
    Assert.Equal(4, resultAsc.Count);
    Assert.Equal(ProjectPriority.Low, resultAsc[0].Priority);
    Assert.Equal(ProjectPriority.Medium, resultAsc[1].Priority);
    Assert.Equal(ProjectPriority.High, resultAsc[2].Priority);
    Assert.Equal(ProjectPriority.Critical, resultAsc[3].Priority);
}

[Fact]
public async Task GetPagedAsync_CombinedSearchFilterAndPagination_ReturnsCorrectResults()
{
    // Arrange
    var project1 = new Project("DevOS API", ProjectPriority.High);
    project1.UpdateStatus(ProjectStatus.Active);

    var project2 = new Project("DevOS Backend", ProjectPriority.High);
    project2.UpdateStatus(ProjectStatus.Active);

    var project3 = new Project("DevOS Frontend", ProjectPriority.Medium);
    project3.UpdateStatus(ProjectStatus.Active);

    var project4 = new Project("DevOS Tests", ProjectPriority.High);
    project4.UpdateStatus(ProjectStatus.Active);

    var project5 = new Project("Game Engine", ProjectPriority.High);
    project5.UpdateStatus(ProjectStatus.Active);

    var project6 = new Project("Other Project", ProjectPriority.High);

    var addRepository = new ProjectRepository(CreateContext());
    await addRepository.AddAsync(project1);
    await addRepository.AddAsync(project2);
    await addRepository.AddAsync(project3);
    await addRepository.AddAsync(project4);
    await addRepository.AddAsync(project5);
    await addRepository.AddAsync(project6);

    var repository = new ProjectRepository(CreateContext());

    // Act
    var result = await repository.GetPagedAsync(
        1, 2, ProjectStatus.Active, ProjectPriority.High, "DevOS", "CreatedAt", "desc");

    // Assert
    Assert.Equal(2, result.Count);
    Assert.All(result, p =>
    {
        Assert.Equal(ProjectStatus.Active, p.Status);
        Assert.Equal(ProjectPriority.High, p.Priority);
        Assert.Contains("DevOS", p.Name);
    });
}
    }
}