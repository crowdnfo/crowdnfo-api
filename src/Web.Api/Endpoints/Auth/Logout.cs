using Application.Abstractions.Messaging;
using Application.Users.Logout;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Auth;

internal sealed class Logout : IEndpoint
{
    public sealed record Request(string RefreshToken);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/logout", async (
            Request request,
            ICommandHandler<LogoutCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new LogoutCommand(request.RefreshToken);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags(EndpointTags.Auth)
        .RequireAuthorization();
    }
}
