using Application.Abstractions.Messaging;
using Application.Users;
using Application.Users.Refresh;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Auth;

internal sealed class Refresh : IEndpoint
{
    public sealed record Request(string RefreshToken);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/refresh", async (
            Request request,
            ICommandHandler<RefreshCommand, AuthResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new RefreshCommand(request.RefreshToken);

            Result<AuthResponse> result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(EndpointTags.Auth);
    }
}
