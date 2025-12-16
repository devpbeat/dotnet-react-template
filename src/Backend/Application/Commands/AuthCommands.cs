using MediatR;

namespace Backend.Application.Commands;

public record RegisterUserCommand(string Email, string Password, string FirstName, string LastName) : IRequest<RegisterUserResult>;

public record RegisterUserResult(Guid UserId, string Email, string Message);

public record LoginUserCommand(string Email, string Password, string IpAddress) : IRequest<LoginUserResult>;

public record LoginUserResult(string AccessToken, string RefreshToken, int ExpiresIn, string TokenType, UserDto User);

public record UserDto(Guid Id, string Email, string FirstName, string LastName, string Role);

public record RefreshTokenCommand(string RefreshToken, string IpAddress) : IRequest<RefreshTokenResult>;

public record RefreshTokenResult(string AccessToken, string RefreshToken, int ExpiresIn, string TokenType);

public record LogoutCommand(string RefreshToken, string IpAddress) : IRequest<bool>;

public record RevokeAllTokensCommand(Guid UserId, string IpAddress) : IRequest<bool>;

public record GetCurrentUserQuery(Guid UserId) : IRequest<UserDto>;
