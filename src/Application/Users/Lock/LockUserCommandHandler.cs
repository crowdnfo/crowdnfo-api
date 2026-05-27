using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.Lock;

internal sealed class LockUserCommandHandler(IApplicationDbContext context)
    : ICommandHandler<LockUserCommand>
{
    public async Task<Result> Handle(LockUserCommand command, CancellationToken cancellationToken)
    {
        User? user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == command.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure(UserErrors.NotFound(command.UserId));
        }

        user.Lock(command.Reason);

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
