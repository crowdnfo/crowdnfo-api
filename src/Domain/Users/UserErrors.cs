using SharedKernel;

namespace Domain.Users;

public static class UserErrors
{
    public static Error NotFound(Guid userId) => Error.NotFound(
        "Users.NotFound",
        $"The user with the Id = '{userId}' was not found");

    public static Error Unauthorized() => Error.Forbidden(
        "Users.Unauthorized",
        "You are not authorized to perform this action.");

    public static readonly Error InvalidCredentials = Error.Unauthorized(
        "Users.InvalidCredentials",
        "Invalid username or password");

    public static readonly Error NotFoundByEmail = Error.NotFound(
        "Users.NotFoundByEmail",
        "The user with the specified email was not found");

    public static readonly Error EmailNotUnique = Error.Conflict(
        "Users.EmailNotUnique",
        "The provided email is not unique");

    public static readonly Error UsernameNotUnique = Error.Conflict(
        "Users.UsernameNotUnique",
        "The provided username is not unique");

    public static readonly Error AlreadyActivated = Error.Conflict(
        "Users.AlreadyActivated",
        "The user is already activated");

    public static readonly Error NotActivated = Error.Forbidden(
        "Users.NotActivated",
        "The user account is not activated");

    public static Error Locked(string? reason) => Error.Forbidden(
        "Users.Locked",
        reason is { Length: > 0 } ? reason : "The user account is locked");
}
