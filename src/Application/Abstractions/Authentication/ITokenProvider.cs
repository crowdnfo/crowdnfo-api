using Domain.Users;

namespace Application.Abstractions.Authentication;

/// <summary><see cref="RefreshToken"/> is the raw value, returned to the client once; only its hash is stored.</summary>
public sealed record TokenPair(
    string AccessToken,
    int ExpiresInSeconds,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc);

public interface ITokenProvider
{
    TokenPair IssueTokens(User user, Role role);
}
