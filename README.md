# DevOS

> A modern developer workspace for managing projects, tasks, and development time.

DevOS is a full-stack productivity platform built for developers who want to keep their projects, tasks, time tracking, and progress in one place.

Built with **ASP.NET Core, React, PostgreSQL, and Docker**.

## Features

### Projects

Create and manage development projects with priorities, deadlines, statuses, and descriptions.

### Tasks

Break projects into manageable tasks and track their progress.

### Time Tracking

Record development sessions and track time spent on projects and individual tasks.

### Analytics

View project activity and development time through aggregated statistics.

### Authentication

Secure registration and login with JWT-based authentication and password hashing.

### Data Isolation

Each user's projects, tasks, and time entries are protected by server-side ownership checks.

## Tech Stack

**Backend**

C# · ASP.NET Core · Entity Framework Core · PostgreSQL

**Frontend**

React · Vite · Tailwind CSS · React Router

**Infrastructure**

Docker · Docker Compose · Nginx

**Testing**

xUnit

## Screenshots

Coming soon.

## Getting Started

### Requirements

* Docker Desktop
* Git

### Installation

```bash
git clone https://github.com/SandrsND1/DevOS.git
cd DevOS
```

Create the environment file:

```powershell
Copy-Item .env.example .env
```

Start DevOS:

```powershell
docker compose up --build
```

Or use:

```powershell
.\run.ps1
```

Open the application:

**http://localhost:3000**

API:

**http://localhost:8080**

## Project Structure

```text
src/
├── DevOS.Domain
├── DevOS.Application
├── DevOS.Infrastructure
├── DevOS.Api
└── DevOS.Client

tests/
├── DevOS.Application.Tests
└── DevOS.Infrastructure.Tests
```

The backend is organized using Clean Architecture principles, separating domain logic, application use cases, infrastructure, and the API layer.

## Testing

Run:

```bash
dotnet test
```

The test suite covers application logic and infrastructure repositories.

## Development

DevOS is currently under active development.

The current focus is on refining the frontend experience, expanding analytics, improving API documentation, and preparing the application for production deployment.

## License

Personal portfolio project.
