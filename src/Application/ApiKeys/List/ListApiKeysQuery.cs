using Application.Abstractions.Messaging;

namespace Application.ApiKeys.List;

public sealed record ListApiKeysQuery : IQuery<IReadOnlyList<ApiKeyResponse>>;

public sealed record ApiKeyResponse(Guid Id, string Name, DateTime CreatedAt);
