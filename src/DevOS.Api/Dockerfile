# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/DevOS.Api/DevOS.Api.csproj", "src/DevOS.Api/"]
COPY ["src/DevOS.Application/DevOS.Application.csproj", "src/DevOS.Application/"]
COPY ["src/DevOS.Domain/DevOS.Domain.csproj", "src/DevOS.Domain/"]
COPY ["src/DevOS.Infrastructure/DevOS.Infrastructure.csproj", "src/DevOS.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "src/DevOS.Api/DevOS.Api.csproj"

# Copy all source code
COPY . .

# Build and publish
WORKDIR "/src/src/DevOS.Api"
RUN dotnet publish "DevOS.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Copy published files
COPY --from=build /app/publish .

# Use built-in non-root user 'app' provided by Microsoft .NET images
USER app

# Expose port
EXPOSE 8080

# Set entry point
ENTRYPOINT ["dotnet", "DevOS.Api.dll"]