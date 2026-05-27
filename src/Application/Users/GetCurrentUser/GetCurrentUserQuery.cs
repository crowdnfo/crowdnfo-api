using Application.Abstractions.Messaging;
using Domain.Users;

namespace Application.Users.GetCurrentUser;

public sealed record GetCurrentUserQuery : IQuery<CurrentUserResponse>;

public sealed record CurrentUserResponse(
    Guid Id,
    string Username,
    string Email,
    string Role,
    ProfileVisibility Visibility,
    bool IsActivated,
    bool IsLocked,
    DateTime CreatedAt);
