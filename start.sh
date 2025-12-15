#!/bin/bash

echo "🚀 Starting Dotnet React Template..."
echo ""
echo "This will start all services with Docker Compose:"
echo "  - PostgreSQL Database (port 5432)"
echo "  - Backend API (port 8080)"
echo "  - Frontend App (port 5173)"
echo ""
echo "Services will be available at:"
echo "  Frontend:  http://localhost:5173"
echo "  Backend:   http://localhost:8080"
echo "  Swagger:   http://localhost:8080/swagger"
echo "  Hangfire:  http://localhost:8080/hangfire"
echo ""
echo "Hot reload is enabled for both frontend and backend!"
echo ""

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    echo "❌ Error: Docker is not running. Please start Docker and try again."
    exit 1
fi

# Check if docker-compose is available
if ! command -v docker-compose &> /dev/null; then
    echo "⚠️  docker-compose not found, trying 'docker compose'..."
    COMPOSE_CMD="docker compose"
else
    COMPOSE_CMD="docker-compose"
fi

# Start services
echo "Starting services..."
$COMPOSE_CMD up --build

# Trap Ctrl+C to clean up
trap '$COMPOSE_CMD down' EXIT
