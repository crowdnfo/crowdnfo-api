using Application.Abstractions.Messaging;
using Application.ApiKeys.List;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.ApiKeys;

internal sealed class ListApiKeys : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("users/me/api-keys", async (
            IQueryHandler<ListApiKeysQuery, IReadOnlyList<ApiKeyResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new ListApiKeysQuery();

            Result<IReadOnlyList<ApiKeyResponse>> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(EndpointTags.ApiKeys)
        .RequireAuthorization();
    }
}
