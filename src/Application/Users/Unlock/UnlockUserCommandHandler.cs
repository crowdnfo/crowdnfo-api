using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.Unlock;

internal sealed class UnlockUserCommandHandler(IApplicationDbContext context)
    : ICommandHandler<UnlockUserCommand>
{
    public async Task<Result> Handle(UnlockUserCommand command, CancellationToken cancellationToken)
    {
        User? user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == command.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure(UserErrors.NotFound(command.UserId));
        }

        user.Unlock();

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
