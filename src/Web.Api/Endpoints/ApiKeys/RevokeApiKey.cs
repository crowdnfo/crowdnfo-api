using Application.Abstractions.Messaging;
using Application.ApiKeys.Revoke;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.ApiKeys;

internal sealed class RevokeApiKey : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("users/me/api-keys/{id:guid}", async (
            Guid id,
            ICommandHandler<RevokeApiKeyCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new RevokeApiKeyCommand(id);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags(EndpointTags.ApiKeys)
        .RequireAuthorization();
    }
}
