using Domain.Users;

namespace Application.Users;

public sealed record AuthResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    AuthUser User);

public sealed record AuthUser(
    Guid Id,
    string Username,
    string Email,
    string Role,
    ProfileVisibility Visibility);
