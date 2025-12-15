using Backend.Domain.Entities;
using System.Security.Claims;

namespace Backend.Application.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    RefreshToken GenerateRefreshToken(Guid userId, string ipAddress);
    ClaimsPrincipal? ValidateAccessToken(string token);
    Task<RefreshToken?> GetRefreshTokenAsync(string token);
    Task RevokeRefreshTokenAsync(string token, string ipAddress, string? replacedByToken = null);
    Task RevokeAllUserTokensAsync(Guid userId, string ipAddress);
    Task<bool> IsRefreshTokenValidAsync(string token);
    Task SaveRefreshTokenAsync(RefreshToken refreshToken);
    Task CleanupExpiredTokensAsync();
}
