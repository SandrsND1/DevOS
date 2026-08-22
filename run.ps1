Write-Host "========================================" -ForegroundColor Cyan
Write-Host "          DevOS Deployment" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# ----------------------------------------
# 1. Check Docker
# ----------------------------------------

Write-Host "`n[1/4] Checking Docker..." -ForegroundColor Yellow

if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    Write-Host ""
    Write-Host "Docker is not installed." -ForegroundColor Red
    Write-Host "Install Docker Desktop and run this script again." -ForegroundColor Red
    exit 1
}

docker info *> $null

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "Docker Desktop is not running." -ForegroundColor Red
    Write-Host "Start Docker Desktop and run this script again." -ForegroundColor Red
    exit 1
}

Write-Host "Docker is ready." -ForegroundColor Green


# ----------------------------------------
# 2. Stop old containers
# ----------------------------------------

Write-Host "`n[2/4] Cleaning previous DevOS containers..." -ForegroundColor Yellow

docker compose down --remove-orphans

if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to stop previous containers." -ForegroundColor Red
    exit 1
}

Write-Host "Previous containers removed." -ForegroundColor Green


# ----------------------------------------
# 3. Build and start DevOS
# ----------------------------------------

Write-Host "`n[3/4] Building and starting DevOS..." -ForegroundColor Yellow

docker compose up --build -d

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "DevOS failed to start." -ForegroundColor Red
    Write-Host ""
    Write-Host "Check logs with:" -ForegroundColor Yellow
    Write-Host "docker compose logs" -ForegroundColor White
    exit 1
}

Write-Host "DevOS containers started." -ForegroundColor Green


# ----------------------------------------
# 4. Check containers
# ----------------------------------------

Write-Host "`n[4/4] Checking DevOS status..." -ForegroundColor Yellow

Start-Sleep -Seconds 5

docker compose ps

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "          DevOS is running!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green

Write-Host ""
Write-Host "Frontend:" -ForegroundColor Cyan
Write-Host "http://localhost:3000" -ForegroundColor White

Write-Host ""
Write-Host "API:" -ForegroundColor Cyan
Write-Host "http://localhost:8080" -ForegroundColor White

Write-Host ""
Write-Host "PostgreSQL:" -ForegroundColor Cyan
Write-Host "localhost:5432" -ForegroundColor White

Write-Host ""
Write-Host "To stop DevOS:" -ForegroundColor Yellow
Write-Host "docker compose down" -ForegroundColor White

Write-Host ""
Write-Host "Opening DevOS..." -ForegroundColor Cyan

Start-Process "http://localhost:3000"