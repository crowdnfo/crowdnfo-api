using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.GetCurrentUser;

internal sealed class GetCurrentUserQueryHandler(
    IApplicationDbContext context,
    IUserContext userContext) : IQueryHandler<GetCurrentUserQuery, CurrentUserResponse>
{
    public async Task<Result<CurrentUserResponse>> Handle(
        GetCurrentUserQuery query,
        CancellationToken cancellationToken)
    {
        CurrentUserResponse? response = await context.Users
            .AsNoTracking()
            .Where(u => u.Id == userContext.UserId)
            .Select(u => new CurrentUserResponse(
                u.Id,
                u.Username,
                u.Email,
                context.Roles.Where(r => r.Id == u.RoleId).Select(r => r.Name).FirstOrDefault()!,
                u.ProfileVisibility,
                u.IsActivated,
                u.IsLocked,
                u.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        if (response is null)
        {
            return Result.Failure<CurrentUserResponse>(UserErrors.NotFound(userContext.UserId));
        }

        return response;
    }
}
