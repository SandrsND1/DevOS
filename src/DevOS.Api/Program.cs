using DevOS.Application.Projects;
using DevOS.Application.Projects.CreateProject;
using DevOS.Application.Projects.GetProjects;
using DevOS.Infrastructure.Persistence;
using DevOS.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Register DbContext with PostgreSQL
builder.Services.AddDbContext<DevOsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DevOS")));

// Register repository
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();

// Register handlers
builder.Services.AddScoped<CreateProjectHandler>();
builder.Services.AddScoped<GetProjectsHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapPost("/api/projects", async (CreateProjectCommand command, CreateProjectHandler handler, CancellationToken cancellationToken) =>
{
    var response = await handler.HandleAsync(command, cancellationToken);
    return Results.Created($"/api/projects/{response.Id}", response);
});

app.MapGet("/api/projects", async (GetProjectsHandler handler, CancellationToken cancellationToken) =>
{
    var query = new GetProjectsQuery();
    var response = await handler.HandleAsync(query, cancellationToken);
    return Results.Ok(response);
});

app.Run();