using Application.Abstractions.Messaging;
using Application.Users.AssignRole;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class AssignRole : IEndpoint
{
    public sealed record Request(int RoleId);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("users/{id:guid}/role", async (
            Guid id,
            Request request,
            ICommandHandler<AssignRoleCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new AssignRoleCommand(id, request.RoleId);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags(EndpointTags.Users)
        .RequireAdmin();
    }
}
