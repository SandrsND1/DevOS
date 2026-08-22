using System.Reflection;
using DevOS.Domain.Entities;
using DevOS.Infrastructure.Persistence;
using DevOS.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DevOS.Infrastructure.Tests
{
    public class ProjectRepositoryTests
    {
        private readonly DbContextOptions<DevOsDbContext> _dbOptions;

        public ProjectRepositoryTests()
        {
            _dbOptions = new DbContextOptionsBuilder<DevOsDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        private DevOsDbContext CreateDbContext() => new DevOsDbContext(_dbOptions);

        private static Project CreateNewProject(string name, string description, Guid userId)
        {
            var project = (Project)Activator.CreateInstance(typeof(Project), nonPublic: true)!;
            
            typeof(Project).GetProperty(nameof(Project.Id))?.SetValue(project, Guid.NewGuid());
            typeof(Project).GetProperty(nameof(Project.Name))?.SetValue(project, name);
            typeof(Project).GetProperty(nameof(Project.Description))?.SetValue(project, description);
            typeof(Project).GetProperty(nameof(Project.UserId))?.SetValue(project, userId);
            typeof(Project).GetProperty(nameof(Project.CreatedAt))?.SetValue(project, DateTime.UtcNow);

            return project;
        }

        [Fact]
        public async Task AddAsync_ShouldAddProjectToDatabase()
        {
            using var context = CreateDbContext();
            var repository = new ProjectRepository(context);
            var userId = Guid.NewGuid();
            var project = CreateNewProject("Test Project", "Description", userId);

            await repository.AddAsync(project);

            var dbProject = await context.Projects.FirstOrDefaultAsync(p => p.Id == project.Id);
            Assert.NotNull(dbProject);
            Assert.Equal("Test Project", dbProject.Name);
            Assert.Equal(userId, dbProject.UserId);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnProject_WhenProjectExists()
        {
            using var context = CreateDbContext();
            var repository = new ProjectRepository(context);
            var userId = Guid.NewGuid();
            var project = CreateNewProject("Test Project", "Description", userId);
            await context.Projects.AddAsync(project);
            await context.SaveChangesAsync();

            // Исправлено: передаем и project.Id, и userId
            var result = await repository.GetByIdAsync(project.Id, userId);

            Assert.NotNull(result);
            Assert.Equal(project.Id, result.Id);
        }

        [Fact]
        public async Task GetPagedAsync_ShouldReturnOnlyUserProjects()
        {
            using var context = CreateDbContext();
            var repository = new ProjectRepository(context);
            var user1 = Guid.NewGuid();
            var user2 = Guid.NewGuid();

            var project1 = CreateNewProject("User 1 Project", "Desc", user1);
            var project2 = CreateNewProject("User 2 Project", "Desc", user2);

            await context.Projects.AddRangeAsync(project1, project2);
            await context.SaveChangesAsync();

            var user1Projects = await repository.GetPagedAsync(userId: user1, page: 1, pageSize: 10);

            Assert.Single(user1Projects);
            Assert.Equal(project1.Id, user1Projects[0].Id);
        }

        [Fact]
        public async Task GetTotalCountAsync_ShouldReturnCorrectCountForUser()
        {
            using var context = CreateDbContext();
            var repository = new ProjectRepository(context);
            var userId = Guid.NewGuid();

            var p1 = CreateNewProject("Project 1", "Desc", userId);
            var p2 = CreateNewProject("Project 2", "Desc", userId);

            await context.Projects.AddRangeAsync(p1, p2);
            await context.SaveChangesAsync();

            var count = await repository.GetTotalCountAsync(userId: userId);

            Assert.Equal(2, count);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveProjectFromDatabase()
        {
            using var context = CreateDbContext();
            var repository = new ProjectRepository(context);
            var userId = Guid.NewGuid();
            var project = CreateNewProject("To Delete", "Desc", userId);
            await context.Projects.AddAsync(project);
            await context.SaveChangesAsync();

            await repository.DeleteAsync(project);

            var dbProject = await context.Projects.FirstOrDefaultAsync(p => p.Id == project.Id);
            Assert.Null(dbProject);
        }
    }
}