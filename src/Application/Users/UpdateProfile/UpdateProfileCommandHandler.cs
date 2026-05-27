using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.UpdateProfile;

internal sealed class UpdateProfileCommandHandler(
    IApplicationDbContext context,
    IUserContext userContext) : ICommandHandler<UpdateProfileCommand>
{
    public async Task<Result> Handle(UpdateProfileCommand command, CancellationToken cancellationToken)
    {
        User? user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure(UserErrors.NotFound(userContext.UserId));
        }

        string normalizedEmail = User.NormalizeEmail(command.Email);

        bool emailTaken = await context.Users
            .AnyAsync(u => u.NormalizedEmail == normalizedEmail && u.Id != user.Id, cancellationToken);

        if (emailTaken)
        {
            return Result.Failure(UserErrors.EmailNotUnique);
        }

        user.ChangeEmail(command.Email);
        user.ChangeVisibility(command.Visibility);

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
