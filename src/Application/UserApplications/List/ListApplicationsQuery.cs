using Application.Abstractions.Messaging;

namespace Application.UserApplications.List;

public sealed record ListApplicationsQuery : IQuery<IReadOnlyList<ApplicationResponse>>;
