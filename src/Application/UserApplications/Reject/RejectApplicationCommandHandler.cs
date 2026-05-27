using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.UserApplications.Reject;

internal sealed class RejectApplicationCommandHandler(
    IApplicationDbContext context,
    IUserContext userContext,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<RejectApplicationCommand>
{
    public async Task<Result> Handle(RejectApplicationCommand command, CancellationToken cancellationToken)
    {
        UserApplication? application = await context.UserApplications
            .FirstOrDefaultAsync(a => a.Id == command.ApplicationId, cancellationToken);

        if (application is null)
        {
            return Result.Failure(UserApplicationErrors.NotFound(command.ApplicationId));
        }

        Result rejection = application.Reject(userContext.UserId, dateTimeProvider.UtcNow, command.Comment);

        if (rejection.IsFailure)
        {
            return rejection;
        }

        User? applicant = await context.Users
            .FirstOrDefaultAsync(u => u.Id == application.UserId, cancellationToken);

        if (applicant is not null)
        {
            context.Users.Remove(applicant);
        }

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
