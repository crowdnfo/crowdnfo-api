using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.ChangePassword;

internal sealed class ChangePasswordCommandHandler(
    IApplicationDbContext context,
    IUserContext userContext,
    IPasswordHasher passwordHasher,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<ChangePasswordCommand>
{
    public async Task<Result> Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        User? user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure(UserErrors.NotFound(userContext.UserId));
        }

        if (!passwordHasher.Verify(command.CurrentPassword, user.PasswordHash))
        {
            return Result.Failure(UserErrors.InvalidCredentials);
        }

        user.ChangePassword(passwordHasher.Hash(command.NewPassword));

        List<RefreshToken> activeTokens = await context.RefreshTokens
            .Where(t => t.UserId == userContext.UserId && t.RevokedAt == null)
            .ToListAsync(cancellationToken);

        RefreshToken.RevokeAll(activeTokens, dateTimeProvider.UtcNow);

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
