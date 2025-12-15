# Dotnet React Template (SaaS Architecture)

## Overview
This project is a modern SaaS template using:
- **Frontend**: React (Vite + TypeScript)
- **Backend**: ASP.NET Core Web API (.NET 10) with Minimal API Endpoints
- **Database**: PostgreSQL (Entity Framework Core)
- **Authentication**: JWT Bearer Tokens
- **Background Jobs**: Hangfire
- **CQRS**: MediatR
- **API Documentation**: Swagger/OpenAPI
- **Billing**: Bancard vPOS 2.0 Integration
- **Container Orchestration**: Docker Compose with hot reload

## Features
- ✅ Minimal API Endpoints (no controllers)
- ✅ JWT Authentication with Swagger integration
- ✅ Hangfire dashboard for background jobs
- ✅ MediatR for CQRS pattern
- ✅ Docker Compose with hot reload for development
- ✅ PostgreSQL database with EF Core
- ✅ CORS configured for frontend

## Structure
```
├── src/
│   ├── Backend/
│   │   ├── Application/          # Application layer
│   │   ├── Domain/               # Domain entities
│   │   ├── Infrastructure/       # Infrastructure (DB, external services)
│   │   ├── Endpoints/            # Minimal API endpoints
│   │   ├── Program.cs            # App configuration
│   │   └── Dockerfile            # Backend Docker config
│   └── Frontend/
│       ├── src/                  # React source code
│       ├── vite.config.ts        # Vite configuration
│       └── Dockerfile            # Frontend Docker config
└── docker-compose.yml            # Docker orchestration
```

## Prerequisites

### Option 1: Docker (Recommended)
- Docker
- Docker Compose

### Option 2: Local Development
- .NET 10 SDK
- Node.js 20+
- PostgreSQL 16

## Quick Start with Docker

1. **Clone and start all services**:
   ```bash
   docker-compose up
   ```

2. **Access the services**:
   - Frontend: http://localhost:5173
   - Backend API: http://localhost:8080
   - Swagger UI: http://localhost:8080/swagger
   - Hangfire Dashboard: http://localhost:8080/hangfire

3. **Hot reload is enabled**:
   - Backend: Changes to .cs files will automatically rebuild
   - Frontend: Changes to React files will hot reload instantly

## Local Development Setup

1. **Database**:
   ```bash
   # Start PostgreSQL (or use Docker)
   docker run -d -p 5432:5432 \
     -e POSTGRES_USER=postgres \
     -e POSTGRES_PASSWORD=postgres \
     -e POSTGRES_DB=dotnet_react_template \
     postgres:16-alpine
   ```

2. **Backend**:
   ```bash
   cd src/Backend
   dotnet restore
   dotnet watch run
   ```
   Access at: http://localhost:8080/swagger

3. **Frontend**:
   ```bash
   cd src/Frontend
   npm install
   npm run dev
   ```
   Access at: http://localhost:5173

## API Documentation

### Authentication
All endpoints (except /auth/login) require JWT authentication.

**Login to get JWT token**:
```bash
curl -X POST http://localhost:8080/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","password":"password"}'
```

Response:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "expiresIn": 3600,
  "email": "user@example.com"
}
```

**Use token in requests**:
```bash
curl -X POST http://localhost:8080/billing/subscribe \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -H "Content-Type: application/json" \
  -d '{"userId":"...","plan":"premium"}'
```

### Available Endpoints

#### Authentication
- `POST /auth/login` - Authenticate and get JWT token

#### Billing
- `POST /billing/subscribe` - Create subscription (requires auth)
- `POST /billing/webhook` - Process Bancard webhook

### Swagger UI
Visit http://localhost:8080/swagger to:
- View all endpoints
- Test API calls with JWT authentication
- See request/response schemas

Click "Authorize" button and enter: `Bearer YOUR_TOKEN`

## Hangfire Dashboard
Access background jobs at: http://localhost:8080/hangfire

Monitor:
- Scheduled jobs
- Recurring jobs
- Job history and failures

## MediatR Usage

Create handlers for commands/queries:

```csharp
// Command
public record CreateSubscriptionCommand(Guid UserId, string Plan) : IRequest<Guid>;

// Handler
public class CreateSubscriptionHandler : IRequestHandler<CreateSubscriptionCommand, Guid>
{
    public async Task<Guid> Handle(CreateSubscriptionCommand request, CancellationToken ct)
    {
        // Implementation
    }
}

// Usage in endpoint
app.MapPost("/subscriptions", async (CreateSubscriptionCommand cmd, IMediator mediator) =>
{
    var id = await mediator.Send(cmd);
    return Results.Ok(id);
});
```

## Configuration

### JWT Settings (appsettings.json)
```json
{
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "DotnetReactTemplate",
    "Audience": "DotnetReactTemplateUsers"
  }
}
```

⚠️ **Production**: Use environment variables or secrets manager for the SecretKey

### Database Connection
- **Docker**: Uses `Host=postgres` (service name)
- **Local**: Uses `Host=localhost` (appsettings.Development.json)

## Docker Commands

```bash
# Start all services
docker-compose up

# Start in background
docker-compose up -d

# View logs
docker-compose logs -f

# Stop all services
docker-compose down

# Rebuild after dependency changes
docker-compose up --build

# Remove volumes (reset database)
docker-compose down -v
```

## Billing Flow
1. User authenticates via `POST /auth/login` to get JWT token
2. User initiates subscription via `POST /billing/subscribe` (with JWT)
3. Backend calls Bancard to get a `process_id`
4. Frontend displays Bancard iframe
5. Bancard calls `POST /billing/webhook` on completion

## Project Highlights

### Minimal API Endpoints
No controllers! All endpoints defined as:
```csharp
app.MapPost("/billing/subscribe", async (SubscribeRequest req, IBancardService svc) =>
{
    var processId = await svc.CreateCatastroRequestAsync(req.UserId, req.Plan);
    return Results.Ok(new { process_id = processId });
})
.WithName("Subscribe")
.WithTags("Billing")
.RequireAuthorization();
```

### Clean Architecture
- **Domain**: Entities, enums, interfaces
- **Application**: Business logic, interfaces
- **Infrastructure**: Database, external services
- **Endpoints**: API definitions

## Contributing
This is a template project. Customize as needed for your SaaS application.

## License
MIT
