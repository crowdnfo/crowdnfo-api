using Application.Abstractions.Messaging;
using Application.UserApplications.Approve;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.UserApplications;

internal sealed class ApproveApplication : IEndpoint
{
    public sealed record Request(string? Comment);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("applications/{id:int}/approve", async (
            int id,
            Request request,
            ICommandHandler<ApproveApplicationCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new ApproveApplicationCommand(id, request.Comment);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags(EndpointTags.Applications)
        .RequireAdmin();
    }
}
