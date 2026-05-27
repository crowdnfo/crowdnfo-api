using Application.Abstractions.Messaging;
using Application.Users.Register;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.UserApplications;

internal sealed class SubmitApplication : IEndpoint
{
    public sealed record Request(string Username, string Email, string Password, string ApplicationData);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("applications", async (
            Request request,
            ICommandHandler<RegisterCommand, RegisterResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new RegisterCommand(
                request.Username,
                request.Email,
                request.Password,
                request.ApplicationData);

            Result<RegisterResponse> result = await handler.Handle(command, cancellationToken);

            return result.Match(
                response => Results.Created((string?)null, response),
                CustomResults.Problem);
        })
        .WithTags(EndpointTags.Applications);
    }
}
