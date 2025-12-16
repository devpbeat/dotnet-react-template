using Backend.Application.Commands;
using Backend.Application.Interfaces;
using MediatR;

namespace Backend.Application.Handlers;

public class AuthHandlers :
    IRequestHandler<RegisterUserCommand, RegisterUserResult>,
    IRequestHandler<LoginUserCommand, LoginUserResult>,
    IRequestHandler<RefreshTokenCommand, RefreshTokenResult>,
    IRequestHandler<LogoutCommand, bool>,
    IRequestHandler<RevokeAllTokensCommand, bool>,
    IRequestHandler<GetCurrentUserQuery, UserDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IAuditService _auditService;

    public AuthHandlers(IUserRepository userRepository, IPasswordHasher passwordHasher, ITokenService tokenService, IAuditService auditService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _auditService = auditService;
    }

    public async Task<RegisterUserResult> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        if (await _userRepository.EmailExistsAsync(request.Email, cancellationToken))
        {
            throw new Exception("User with this email already exists");
        }

        var user = new Backend.Domain.Entities.User
        {
            Email = request.Email,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Role = "User",
            IsActive = true
        };

        await _userRepository.AddAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("UserCreated", "User", $"User {user.Email} created", user.Id);

        return new RegisterUserResult(user.Id, user.Email, "User registered successfully");
    }

    public async Task<LoginUserResult> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user == null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("User account is inactive");
        }

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken(user.Id, request.IpAddress);

        await _tokenService.SaveRefreshTokenAsync(refreshToken);

        await _userRepository.UpdateLastLoginAsync(user.Id, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Login", "User", "User logged in", user.Id, request.IpAddress);

        return new LoginUserResult(
            accessToken,
            refreshToken.Token,
            900,
            "Bearer",
            new UserDto(user.Id, user.Email, user.FirstName, user.LastName, user.Role)
        );
    }

    public async Task<RefreshTokenResult> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var storedToken = await _tokenService.GetRefreshTokenAsync(request.RefreshToken);
        if (storedToken == null || !storedToken.IsActive)
        {
            throw new UnauthorizedAccessException("Invalid refresh token");
        }

        var user = await _userRepository.GetByIdAsync(storedToken.UserId, cancellationToken);
        if (user == null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("User not found or inactive");
        }

        var newAccessToken = _tokenService.GenerateAccessToken(user);
        var newRefreshToken = _tokenService.GenerateRefreshToken(user.Id, request.IpAddress);

        await _tokenService.RevokeRefreshTokenAsync(storedToken.Token, request.IpAddress, newRefreshToken.Token);
        await _tokenService.SaveRefreshTokenAsync(newRefreshToken);

        return new RefreshTokenResult(newAccessToken, newRefreshToken.Token, 900, "Bearer");
    }

    public async Task<bool> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _tokenService.RevokeRefreshTokenAsync(request.RefreshToken, request.IpAddress);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> Handle(RevokeAllTokensCommand request, CancellationToken cancellationToken)
    {
        await _tokenService.RevokeAllUserTokensAsync(request.UserId, request.IpAddress);
        return true;
    }

    public async Task<UserDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }

        return new UserDto(user.Id, user.Email, user.FirstName, user.LastName, user.Role);
    }
}
