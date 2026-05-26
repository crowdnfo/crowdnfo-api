using SharedKernel;

namespace Domain.Users;

public static class RefreshTokenErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "RefreshTokens.NotFound",
        "The refresh token was not found");

    public static readonly Error Expired = Error.Failure(
        "RefreshTokens.Expired",
        "The refresh token has expired");

    public static readonly Error AlreadyRevoked = Error.Failure(
        "RefreshTokens.AlreadyRevoked",
        "The refresh token has already been revoked");
}
