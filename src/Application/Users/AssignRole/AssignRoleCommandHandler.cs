using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.AssignRole;

internal sealed class AssignRoleCommandHandler(IApplicationDbContext context)
    : ICommandHandler<AssignRoleCommand>
{
    public async Task<Result> Handle(AssignRoleCommand command, CancellationToken cancellationToken)
    {
        User? user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == command.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure(UserErrors.NotFound(command.UserId));
        }

        if (!await context.Roles.AnyAsync(r => r.Id == command.RoleId, cancellationToken))
        {
            return Result.Failure(RoleErrors.NotFound(command.RoleId));
        }

        user.AssignRole(command.RoleId);

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
