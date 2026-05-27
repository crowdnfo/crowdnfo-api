using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.Activate;

internal sealed class ActivateUserCommandHandler(IApplicationDbContext context)
    : ICommandHandler<ActivateUserCommand>
{
    public async Task<Result> Handle(ActivateUserCommand command, CancellationToken cancellationToken)
    {
        User? user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == command.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure(UserErrors.NotFound(command.UserId));
        }

        Result activationResult = user.Activate();

        if (activationResult.IsFailure)
        {
            return activationResult;
        }

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
