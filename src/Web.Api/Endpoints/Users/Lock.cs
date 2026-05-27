using Application.Abstractions.Messaging;
using Application.Users.Lock;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class Lock : IEndpoint
{
    public sealed record Request(string? Reason);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("users/{id:guid}/lock", async (
            Guid id,
            Request request,
            ICommandHandler<LockUserCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new LockUserCommand(id, request.Reason);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags(EndpointTags.Users)
        .RequireAdmin();
    }
}
