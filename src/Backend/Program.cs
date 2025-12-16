using System.Text;
using Backend.Application.Interfaces;
using Backend.Application.Jobs;
using Backend.Endpoints;
using Backend.Infrastructure.Bancard;
using Backend.Infrastructure.Configuration;
using Backend.Infrastructure.Middleware;
using Backend.Infrastructure.Persistence;
using Backend.Infrastructure.Repositories;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Configure secrets management (Infisical or Environment Variables)
await builder.ConfigureSecretsAsync();

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Configure Swagger with JWT support
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Dotnet React Template API",
        Version = "v1",
        Description = "API for Dotnet React Template with JWT Authentication"
    });

    // Add JWT Authentication to Swagger
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configure JWT Authentication
// Secrets are loaded from environment variables or Infisical
var secretKey = builder.Configuration.GetSecretOrDefault("JWT_SECRET_KEY")
    ?? builder.Configuration.GetSection("JwtSettings")["SecretKey"]
    ?? throw new InvalidOperationException("JWT Secret Key not configured");

var jwtIssuer = builder.Configuration.GetSecretOrDefault("JWT_ISSUER")
    ?? builder.Configuration.GetSection("JwtSettings")["Issuer"]
    ?? "DotnetReactTemplate";

var jwtAudience = builder.Configuration.GetSecretOrDefault("JWT_AUDIENCE")
    ?? builder.Configuration.GetSection("JwtSettings")["Audience"]
    ?? "DotnetReactTemplateUsers";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };    
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (context.Request.Cookies.ContainsKey("access_token"))
            {
                context.Token = context.Request.Cookies["access_token"];
            }
            return Task.CompletedTask;
        }
    };});

builder.Services.AddAuthorization();

// Configure Database
var connectionString = builder.Configuration.GetSecretOrDefault("DATABASE_URL")
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Database connection string not configured");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString)
           .AddInterceptors(new UpdateTimestampInterceptor()));

// Configure Hangfire
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(options =>
        options.UseNpgsqlConnection(connectionString)));

builder.Services.AddHangfireServer();

// Configure MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Register repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Register application services
builder.Services.AddScoped<IBancardService, BancardService>();
builder.Services.AddScoped<Backend.Application.Interfaces.ITokenService, Backend.Infrastructure.Services.TokenService>();
builder.Services.AddScoped<Backend.Application.Interfaces.IPasswordHasher, Backend.Infrastructure.Services.PasswordHasher>();
builder.Services.AddScoped<IAuditService, Backend.Infrastructure.Services.AuditService>();

// Register background jobs
builder.Services.AddScoped<Backend.Application.Jobs.SampleBackgroundJob>();
builder.Services.AddScoped<Backend.Application.Jobs.TokenCleanupJob>();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173") // Allow Vite frontend
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // Required for cookies/auth
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Dotnet React Template API v1");
    });
}

app.UseCors("AllowFrontend");

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<AuditMiddleware>();

// Map Hangfire Dashboard
if (app.Environment.IsDevelopment())
{
    app.MapHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = new[] { new HangfireAuthorizationFilter() }
    });
}
else
{
    app.MapHangfireDashboard("/hangfire");
}

// Map API Endpoints
app.MapAuthEndpoints();
app.MapBillingEndpoints();
app.MapExampleEndpoints();

// Register Hangfire recurring jobs
app.RegisterRecurringJobs();

app.Run();

