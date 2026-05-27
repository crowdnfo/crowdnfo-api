using Application.Abstractions.Messaging;
using Application.UserApplications.List;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.UserApplications;

internal sealed class ListApplications : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("applications", async (
            IQueryHandler<ListApplicationsQuery, IReadOnlyList<ApplicationResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new ListApplicationsQuery();

            Result<IReadOnlyList<ApplicationResponse>> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(EndpointTags.Applications)
        .RequireAdmin();
    }
}
