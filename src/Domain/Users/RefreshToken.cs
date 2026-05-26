using SharedKernel;

namespace Domain.Users;

public sealed class RefreshToken : IHasTimestamps
{
    private RefreshToken()
    {
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public string TokenHash { get; private set; }

    public DateTime ExpiresAt { get; private set; }

    public DateTime? RevokedAt { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public static RefreshToken Issue(Guid userId, string tokenHash, DateTime expiresAt) =>
        new()
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresAt = expiresAt
        };

    public bool IsActive(DateTime now) => RevokedAt is null && ExpiresAt > now;

    public Result Revoke(DateTime now)
    {
        if (RevokedAt is not null)
        {
            return Result.Failure(RefreshTokenErrors.AlreadyRevoked);
        }

        RevokedAt = now;

        return Result.Success();
    }
}
