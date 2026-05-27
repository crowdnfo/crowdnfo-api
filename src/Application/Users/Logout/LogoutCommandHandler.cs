using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.Logout;

internal sealed class LogoutCommandHandler(
    IApplicationDbContext context,
    IUserContext userContext,
    ITokenHasher tokenHasher,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<LogoutCommand>
{
    public async Task<Result> Handle(LogoutCommand command, CancellationToken cancellationToken)
    {
        string tokenHash = tokenHasher.Hash(command.RefreshToken);

        RefreshToken? token = await context.RefreshTokens
            .FirstOrDefaultAsync(
                t => t.UserId == userContext.UserId && t.TokenHash == tokenHash,
                cancellationToken);

        if (token is not null)
        {
            token.Revoke(dateTimeProvider.UtcNow);
            await context.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }
}
