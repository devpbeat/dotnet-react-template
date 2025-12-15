@echo off
echo.
echo 🚀 Starting Dotnet React Template...
echo.
echo This will start all services with Docker Compose:
echo   - PostgreSQL Database (port 5432)
echo   - Backend API (port 8080)
echo   - Frontend App (port 5173)
echo.
echo Services will be available at:
echo   Frontend:  http://localhost:5173
echo   Backend:   http://localhost:8080
echo   Swagger:   http://localhost:8080/swagger
echo   Hangfire:  http://localhost:8080/hangfire
echo.
echo Hot reload is enabled for both frontend and backend!
echo.

REM Check if Docker is running
docker info >nul 2>&1
if errorlevel 1 (
    echo ❌ Error: Docker is not running. Please start Docker and try again.
    exit /b 1
)

echo Starting services...
docker-compose up --build

pause
