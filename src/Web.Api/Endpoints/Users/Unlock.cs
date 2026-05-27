using Application.Abstractions.Messaging;
using Application.Users.Unlock;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class Unlock : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("users/{id:guid}/unlock", async (
            Guid id,
            ICommandHandler<UnlockUserCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new UnlockUserCommand(id);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags(EndpointTags.Users)
        .RequireAdmin();
    }
}
