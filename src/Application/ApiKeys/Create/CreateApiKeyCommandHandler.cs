using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.ApiKeys.Create;

internal sealed class CreateApiKeyCommandHandler(
    IApplicationDbContext context,
    IUserContext userContext,
    ISecureTokenGenerator tokenGenerator,
    ITokenHasher tokenHasher) : ICommandHandler<CreateApiKeyCommand, CreateApiKeyResponse>
{
    public async Task<Result<CreateApiKeyResponse>> Handle(
        CreateApiKeyCommand command,
        CancellationToken cancellationToken)
    {
        bool nameTaken = await context.ApiKeys
            .AnyAsync(k => k.UserId == userContext.UserId && k.Name == command.Name, cancellationToken);

        if (nameTaken)
        {
            return Result.Failure<CreateApiKeyResponse>(ApiKeyErrors.NameNotUnique);
        }

        string rawKey = $"cnfo_{tokenGenerator.Generate()}";

        var apiKey = ApiKey.Create(userContext.UserId, command.Name, tokenHasher.Hash(rawKey));

        context.ApiKeys.Add(apiKey);

        await context.SaveChangesAsync(cancellationToken);

        return new CreateApiKeyResponse(apiKey.Id, apiKey.Name, rawKey);
    }
}
