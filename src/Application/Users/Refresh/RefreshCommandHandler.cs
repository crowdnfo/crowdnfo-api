using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.Refresh;

internal sealed class RefreshCommandHandler(
    IApplicationDbContext context,
    ITokenProvider tokenProvider,
    ITokenHasher tokenHasher,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<RefreshCommand, AuthResponse>
{
    public async Task<Result<AuthResponse>> Handle(RefreshCommand command, CancellationToken cancellationToken)
    {
        string tokenHash = tokenHasher.Hash(command.RefreshToken);
        DateTime now = dateTimeProvider.UtcNow;

        RefreshToken? token = await context.RefreshTokens
            .FirstOrDefaultAsync(refreshToken => refreshToken.TokenHash == tokenHash, cancellationToken);

        if (token is null)
        {
            return Result.Failure<AuthResponse>(RefreshTokenErrors.Invalid);
        }

        if (!token.IsActive(now))
        {
            await RevokeChainAsync(token.UserId, now, cancellationToken);
            return Result.Failure<AuthResponse>(RefreshTokenErrors.Invalid);
        }

        User? user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == token.UserId, cancellationToken);

        if (user is null || user.EnsureCanSignIn().IsFailure)
        {
            await RevokeChainAsync(token.UserId, now, cancellationToken);
            return Result.Failure<AuthResponse>(RefreshTokenErrors.Invalid);
        }

        Role? role = await context.Roles
            .FirstOrDefaultAsync(r => r.Id == user.RoleId, cancellationToken);

        if (role is null)
        {
            return Result.Failure<AuthResponse>(RoleErrors.NotFound(user.RoleId));
        }

        token.Revoke(now);

        TokenPair tokens = tokenProvider.IssueTokens(user, role);

        context.RefreshTokens.Add(RefreshToken.Issue(
            user.Id,
            tokenHasher.Hash(tokens.RefreshToken),
            tokens.RefreshTokenExpiresAtUtc));

        await context.SaveChangesAsync(cancellationToken);

        return new AuthResponse(
            tokens.AccessToken,
            tokens.RefreshToken,
            tokens.ExpiresInSeconds,
            new AuthUser(user.Id, user.Username, user.Email, role.Name, user.ProfileVisibility));
    }

    private async Task RevokeChainAsync(Guid userId, DateTime now, CancellationToken cancellationToken)
    {
        List<RefreshToken> activeTokens = await context.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null)
            .ToListAsync(cancellationToken);

        RefreshToken.RevokeAll(activeTokens, now);

        await context.SaveChangesAsync(cancellationToken);
    }
}
