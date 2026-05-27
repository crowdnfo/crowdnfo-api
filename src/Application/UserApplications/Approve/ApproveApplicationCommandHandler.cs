using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.UserApplications.Approve;

internal sealed class ApproveApplicationCommandHandler(
    IApplicationDbContext context,
    IUserContext userContext,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<ApproveApplicationCommand>
{
    public async Task<Result> Handle(ApproveApplicationCommand command, CancellationToken cancellationToken)
    {
        UserApplication? application = await context.UserApplications
            .FirstOrDefaultAsync(a => a.Id == command.ApplicationId, cancellationToken);

        if (application is null)
        {
            return Result.Failure(UserApplicationErrors.NotFound(command.ApplicationId));
        }

        Result approval = application.Approve(userContext.UserId, dateTimeProvider.UtcNow, command.Comment);

        if (approval.IsFailure)
        {
            return approval;
        }

        User? applicant = await context.Users
            .FirstOrDefaultAsync(u => u.Id == application.UserId, cancellationToken);

        if (applicant is not null)
        {
            Result activation = applicant.Activate();

            if (activation.IsFailure)
            {
                return activation;
            }
        }

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
