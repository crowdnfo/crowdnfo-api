using SharedKernel;

namespace Domain.Users;

public static class RefreshTokenErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "RefreshTokens.NotFound",
        "The refresh token was not found");

    public static readonly Error Expired = Error.Unauthorized(
        "RefreshTokens.Expired",
        "The refresh token has expired");

    public static readonly Error AlreadyRevoked = Error.Unauthorized(
        "RefreshTokens.AlreadyRevoked",
        "The refresh token has already been revoked");

    public static readonly Error Invalid = Error.Unauthorized(
        "RefreshTokens.Invalid",
        "The refresh token is invalid");
}
