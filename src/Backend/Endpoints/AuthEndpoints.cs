using System.Security.Claims;
using Backend.Application.Commands;
using Backend.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth")
            .WithTags("Authentication");

        group.MapPost("/register", async (
            [FromBody] RegisterRequest request,
            IMediator mediator) =>
        {
            var command = new RegisterUserCommand(request.Email, request.Password, request.FirstName, request.LastName);
            var result = await mediator.Send(command);
            return Results.Ok(result);
        })
        .WithName("Register")
        .WithDescription("Register a new user")
        .AllowAnonymous();

        group.MapPost("/login", async (
            [FromBody] LoginRequest request,
            IMediator mediator,
            HttpContext httpContext) =>
        {
            var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var command = new LoginUserCommand(request.Email, request.Password, ipAddress);
            var result = await mediator.Send(command);

            // Set refresh token in HttpOnly cookie
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // Set to true in production
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            };
            httpContext.Response.Cookies.Append("refresh_token", result.RefreshToken, cookieOptions);
            
            // Also set access token in cookie for simplicity if frontend expects it there
            // But typically access token is returned in body for Bearer auth
            // The previous AuthController used access_token cookie. 
            // Let's stick to the previous behavior for compatibility with the frontend I wrote.
            
            var accessCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddMinutes(15)
            };
            httpContext.Response.Cookies.Append("access_token", result.AccessToken, accessCookieOptions);

            return Results.Ok(new { message = "Login successful", user = result.User });
        })
        .WithName("Login")
        .WithDescription("Authenticate user")
        .AllowAnonymous();

        group.MapPost("/refresh", async (
            IMediator mediator,
            HttpContext httpContext) =>
        {
            var refreshToken = httpContext.Request.Cookies["refresh_token"];
            if (string.IsNullOrEmpty(refreshToken))
            {
                return Results.Unauthorized();
            }

            var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var command = new RefreshTokenCommand(refreshToken, ipAddress);
            var result = await mediator.Send(command);

            // Update cookies
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            };
            httpContext.Response.Cookies.Append("refresh_token", result.RefreshToken, cookieOptions);
            
            var accessCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddMinutes(15)
            };
            httpContext.Response.Cookies.Append("access_token", result.AccessToken, accessCookieOptions);

            return Results.Ok(new { message = "Token refreshed" });
        })
        .WithName("RefreshToken")
        .WithDescription("Refresh access token using refresh token from cookie")
        .AllowAnonymous();

        group.MapPost("/logout", async (
            IMediator mediator,
            HttpContext httpContext) =>
        {
            var refreshToken = httpContext.Request.Cookies["refresh_token"];
            var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            if (!string.IsNullOrEmpty(refreshToken))
            {
                await mediator.Send(new LogoutCommand(refreshToken, ipAddress));
            }

            httpContext.Response.Cookies.Delete("refresh_token");
            httpContext.Response.Cookies.Delete("access_token");
            
            return Results.Ok(new { message = "Logged out successfully" });
        })
        .WithName("Logout")
        .WithDescription("Logout user")
        .AllowAnonymous();

        group.MapGet("/me", async (
            IMediator mediator,
            HttpContext httpContext) =>
        {
            var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return Results.Unauthorized();
            }

            var result = await mediator.Send(new GetCurrentUserQuery(userId));
            return Results.Ok(result);
        })
        .WithName("GetCurrentUser")
        .WithDescription("Get current authenticated user information")
        .RequireAuthorization();
    }
}

