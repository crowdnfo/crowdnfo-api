using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.ApiKeys.Revoke;

internal sealed class RevokeApiKeyCommandHandler(
    IApplicationDbContext context,
    IUserContext userContext) : ICommandHandler<RevokeApiKeyCommand>
{
    public async Task<Result> Handle(RevokeApiKeyCommand command, CancellationToken cancellationToken)
    {
        ApiKey? apiKey = await context.ApiKeys
            .FirstOrDefaultAsync(
                k => k.Id == command.ApiKeyId && k.UserId == userContext.UserId,
                cancellationToken);

        if (apiKey is null)
        {
            return Result.Failure(ApiKeyErrors.NotFound(command.ApiKeyId));
        }

        context.ApiKeys.Remove(apiKey);

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
