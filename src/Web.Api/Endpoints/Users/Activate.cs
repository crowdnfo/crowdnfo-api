using Application.Abstractions.Messaging;
using Application.Users.Activate;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class Activate : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("users/{id:guid}/activate", async (
            Guid id,
            ICommandHandler<ActivateUserCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new ActivateUserCommand(id);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags(EndpointTags.Users)
        .RequireAdmin();
    }
}
