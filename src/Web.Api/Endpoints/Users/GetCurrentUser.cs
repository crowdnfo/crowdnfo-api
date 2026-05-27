using Application.Abstractions.Messaging;
using Application.Users.GetCurrentUser;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class GetCurrentUser : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("users/me", async (
            IQueryHandler<GetCurrentUserQuery, CurrentUserResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetCurrentUserQuery();

            Result<CurrentUserResponse> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(EndpointTags.Users)
        .RequireAuthorization();
    }
}
