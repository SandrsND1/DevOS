using DevOS.Domain.Entities;
using DevOS.Infrastructure.Persistence;
using DevOS.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DevOS.Infrastructure.Tests
{
    public class TimeEntryRepositoryTests
    {
        private readonly DbContextOptions<DevOsDbContext> _dbOptions;

        public TimeEntryRepositoryTests()
        {
            _dbOptions = new DbContextOptionsBuilder<DevOsDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        private DevOsDbContext CreateDbContext() => new DevOsDbContext(_dbOptions);

        private static Project CreateTestProject(string name, string desc)
        {
            var project = (Project)Activator.CreateInstance(typeof(Project), nonPublic: true)!;
            typeof(Project).GetProperty(nameof(Project.Id))?.SetValue(project, Guid.NewGuid());
            typeof(Project).GetProperty(nameof(Project.Name))?.SetValue(project, name);
            typeof(Project).GetProperty(nameof(Project.Description))?.SetValue(project, desc);
            return project;
        }

        private static TimeEntry CreateTestTimeEntry(Guid projectId, DateTime start, DateTime end, string desc)
        {
            var entry = (TimeEntry)Activator.CreateInstance(typeof(TimeEntry), nonPublic: true)!;
            typeof(TimeEntry).GetProperty(nameof(TimeEntry.Id))?.SetValue(entry, Guid.NewGuid());
            typeof(TimeEntry).GetProperty(nameof(TimeEntry.ProjectId))?.SetValue(entry, projectId);
            typeof(TimeEntry).GetProperty(nameof(TimeEntry.StartedAt))?.SetValue(entry, start);
            typeof(TimeEntry).GetProperty(nameof(TimeEntry.EndedAt))?.SetValue(entry, end);
            typeof(TimeEntry).GetProperty(nameof(TimeEntry.Description))?.SetValue(entry, desc);
            return entry;
        }

        [Fact]
        public async Task AddAsync_ShouldPersistTimeEntry()
        {
            using var context = CreateDbContext();
            var repository = new TimeEntryRepository(context);
            var project = CreateTestProject("Test Project", "Desc");
            await context.Projects.AddAsync(project);
            await context.SaveChangesAsync();

            var now = DateTime.UtcNow;
            var entry = CreateTestTimeEntry(project.Id, now.AddHours(-2), now, "Coding session");

            await repository.AddAsync(entry);

            var dbEntry = await context.TimeEntries.FirstOrDefaultAsync(e => e.Id == entry.Id);
            Assert.NotNull(dbEntry);
            Assert.Equal(project.Id, dbEntry.ProjectId);
            Assert.Equal("Coding session", dbEntry.Description);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnTimeEntry_WhenExists()
        {
            using var context = CreateDbContext();
            var repository = new TimeEntryRepository(context);
            var project = CreateTestProject("Test Project", "Desc");
            await context.Projects.AddAsync(project);
            await context.SaveChangesAsync();

            var now = DateTime.UtcNow;
            var entry = CreateTestTimeEntry(project.Id, now.AddHours(-1), now, "Code review");
            await context.TimeEntries.AddAsync(entry);
            await context.SaveChangesAsync();

            // Передаем один аргумент id согласно контракту ITimeEntryRepository
            var result = await repository.GetByIdAsync(entry.Id);

            Assert.NotNull(result);
            Assert.Equal(entry.Id, result.Id);
            Assert.Equal(project.Id, result.ProjectId);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveTimeEntry()
        {
            using var context = CreateDbContext();
            var repository = new TimeEntryRepository(context);
            var project = CreateTestProject("Test Project", "Desc");
            await context.Projects.AddAsync(project);
            await context.SaveChangesAsync();

            var now = DateTime.UtcNow;
            var entry = CreateTestTimeEntry(project.Id, now.AddHours(-1), now, "To Delete");
            await context.TimeEntries.AddAsync(entry);
            await context.SaveChangesAsync();

            await repository.DeleteAsync(entry);

            var dbEntry = await context.TimeEntries.FirstOrDefaultAsync(e => e.Id == entry.Id);
            Assert.Null(dbEntry);
        }
    }
}