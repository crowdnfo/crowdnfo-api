using Application.Abstractions.Messaging;
using Application.ApiKeys.Create;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.ApiKeys;

internal sealed class CreateApiKey : IEndpoint
{
    public sealed record Request(string Name);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("users/me/api-keys", async (
            Request request,
            ICommandHandler<CreateApiKeyCommand, CreateApiKeyResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateApiKeyCommand(request.Name);

            Result<CreateApiKeyResponse> result = await handler.Handle(command, cancellationToken);

            return result.Match(
                response => Results.Created((string?)null, response),
                CustomResults.Problem);
        })
        .WithTags(EndpointTags.ApiKeys)
        .RequireAuthorization();
    }
}
