# Dotnet React Template (SaaS Architecture)

## Overview
This project is a modern SaaS template using:
- **Frontend**: React (Vite + TypeScript)
- **Backend**: ASP.NET Core Web API (.NET 10) with Minimal API Endpoints
- **Database**: PostgreSQL (Entity Framework Core)
- **Authentication**: Production-Ready JWT with Refresh Tokens
- **Background Jobs**: Hangfire
- **CQRS**: MediatR
- **API Documentation**: Swagger/OpenAPI
- **Billing**: Bancard vPOS 2.0 Integration
- **Container Orchestration**: Docker Compose with hot reload

## Features
- ✅ Minimal API Endpoints (no controllers)
- ✅ **Production-Ready JWT Authentication**
  - Access tokens (15-min expiry)
  - Refresh tokens with rotation (7-day expiry)
  - PBKDF2 password hashing (100k iterations)
  - Token revocation & IP tracking
  - Automatic expired token cleanup
- ✅ Hangfire dashboard for background jobs
- ✅ MediatR for CQRS pattern
- ✅ Docker Compose with hot reload for development
- ✅ PostgreSQL database with EF Core
- ✅ CORS configured for frontend
- ✅ Swagger UI with JWT authorization

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

### 🔐 Authentication (Production-Ready)

**Quick Start**: See [QUICK_START_AUTH.md](./QUICK_START_AUTH.md) for a 5-minute guided tutorial.

**Full Guide**: See [JWT_AUTH_GUIDE.md](./JWT_AUTH_GUIDE.md) for complete documentation.

#### Register & Login

1. **Register** a new user:
```bash
curl -X POST http://localhost:8080/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"Password123!","firstName":"John","lastName":"Doe"}'
```

2. **Login** to get tokens:
```bash
curl -X POST http://localhost:8080/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"Password123!"}'
```

Response:
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "base64-encoded-refresh-token...",
  "expiresIn": 900,
  "tokenType": "Bearer",
  "user": {
    "id": "guid",
    "email": "test@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "role": "User"
  }
}
```

3. **Use access token** in API requests:
```bash
curl -X GET http://localhost:8080/auth/me \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN_HERE"
```

4. **Refresh** when access token expires (after 15 min):
```bash
curl -X POST http://localhost:8080/auth/refresh \
  -H "Content-Type: application/json" \
  -d '{"refreshToken":"YOUR_REFRESH_TOKEN_HERE"}'
```

### Available Endpoints

#### Authentication
- `POST /auth/register` - Register new user
- `POST /auth/login` - Login and get access + refresh tokens
- `POST /auth/refresh` - Refresh access token using refresh token
- `POST /auth/logout` - Logout by revoking refresh token
- `GET /auth/me` - Get current user info (requires auth)
- `POST /auth/revoke-all` - Revoke all user's tokens (requires auth)

#### Billing
- `POST /billing/subscribe` - Create subscription (requires auth)
- `POST /billing/webhook` - Process Bancard webhook

#### Examples (MediatR + Hangfire)
- `POST /examples/subscriptions/create` - MediatR CQRS example
- `POST /examples/jobs/subscription-renewal` - Fire-and-forget job
- `POST /examples/jobs/welcome-email` - Delayed job example
- `POST /examples/subscriptions/create-with-email` - Combined MediatR + Hangfire

### Swagger UI
Visit http://localhost:8080/swagger to:
- View all endpoints
- Test API calls with JWT authentication
- See request/response schemas

Click "Authorize" button and enter: `Bearer YOUR_TOKEN`

## Hangfire Dashboard
Access background jobs at: http://localhost:8080/hangfire

Monitor:
- **Token Cleanup Job**: Automatically removes expired refresh tokens (runs hourly)
- **Subscription Cleanup Job**: Example job (runs daily at 2 AM)
- Scheduled and recurring jobs
- Job history and failures
- Real-time job execution monitoring

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
