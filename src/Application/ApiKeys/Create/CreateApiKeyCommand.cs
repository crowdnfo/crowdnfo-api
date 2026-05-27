using Application.Abstractions.Messaging;

namespace Application.ApiKeys.Create;

public sealed record CreateApiKeyCommand(string Name) : ICommand<CreateApiKeyResponse>;

public sealed record CreateApiKeyResponse(Guid Id, string Name, string Key);
