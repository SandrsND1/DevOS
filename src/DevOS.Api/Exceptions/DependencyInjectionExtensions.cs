using System.Text;
using DevOS.Application.Abstractions.Repositories;
using DevOS.Application.Abstractions.Services;
using DevOS.Application.Analytics.Queries;
using DevOS.Application.Projects.Commands.ChangeProjectStatus;
using DevOS.Application.Projects.Commands.CreateProject;
using DevOS.Application.Projects.Commands.DeleteProject;
using DevOS.Application.Projects.Commands.UpdateProject;
using DevOS.Application.Projects.Queries.GetProjectById;
using DevOS.Application.Projects.Queries.GetProjects;
using DevOS.Application.Tasks;
using DevOS.Application.Tasks.CreateTask;
using DevOS.Application.Tasks.DeleteTask;
using DevOS.Application.Tasks.GetTask;
using DevOS.Application.Tasks.GetTasks;
using DevOS.Application.Tasks.UpdateTask;
using DevOS.Application.TimeEntries;
using DevOS.Application.TimeEntries.Commands.CreateTimeEntry;
using DevOS.Application.TimeEntries.Commands.DeleteTimeEntry;
using DevOS.Application.TimeEntries.Commands.UpdateTimeEntry;
using DevOS.Application.TimeEntries.Queries.GetTimeEntries;
using DevOS.Application.TimeEntries.Queries.GetTimeEntryById;
using DevOS.Application.Users;
using DevOS.Application.Users.Commands;
using DevOS.Infrastructure.Persistence;
using DevOS.Infrastructure.Persistence.Repositories;
using DevOS.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace DevOS.Api.Extensions
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddDevOsServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // DbContext
            services.AddDbContext<DevOsDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DevOS")));

            // Repositories
            services.AddScoped<IProjectRepository, ProjectRepository>();
            services.AddScoped<ITaskRepository, TaskRepository>();
            services.AddScoped<ITimeEntryRepository, TimeEntryRepository>();
            services.AddScoped<IUserRepository, UserRepository>();

            // Services & Http Context
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

            // Validators
            services.AddScoped<CreateTaskValidator>();
            services.AddScoped<GetTasksValidator>();
            services.AddScoped<UpdateTaskValidator>();
            services.AddScoped<CreateTimeEntryValidator>();
            services.AddScoped<UpdateTimeEntryValidator>();
            services.AddScoped<GetTimeEntriesValidator>();
            services.AddScoped<RegisterUserValidator>();
            services.AddScoped<LoginValidator>();

            // Project Handlers
            services.AddScoped<CreateProjectHandler>();
            services.AddScoped<GetProjectByIdHandler>();
            services.AddScoped<GetProjectsHandler>();
            services.AddScoped<UpdateProjectHandler>();
            services.AddScoped<DeleteProjectHandler>();
            services.AddScoped<ChangeProjectStatusHandler>();

            // Task Handlers
            services.AddScoped<CreateTaskHandler>();
            services.AddScoped<GetTaskHandler>();
            services.AddScoped<GetTasksHandler>();
            services.AddScoped<UpdateTaskHandler>();
            services.AddScoped<DeleteTaskHandler>();

            // TimeEntry Handlers
            services.AddScoped<CreateTimeEntryHandler>();
            services.AddScoped<GetTimeEntryByIdHandler>();
            services.AddScoped<GetTimeEntriesHandler>();
            services.AddScoped<UpdateTimeEntryHandler>();
            services.AddScoped<DeleteTimeEntryHandler>();

            // Analytics & Users Handlers
            services.AddScoped<GetProjectAnalyticsHandler>();
            services.AddScoped<RegisterUserHandler>();
            services.AddScoped<LoginHandler>();

            return services;
        }

        public static IServiceCollection AddJwtAuthentication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var secretKey = configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("JWT signing key is not configured.");
            var key = Encoding.ASCII.GetBytes(secretKey);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = configuration["Jwt:Issuer"] ?? "DevOS",
                    ValidateAudience = true,
                    ValidAudience = configuration["Jwt:Audience"] ?? "DevOS-Users",
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

            return services;
        }
    }
}