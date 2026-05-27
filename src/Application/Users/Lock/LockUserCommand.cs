using Application.Abstractions.Messaging;

namespace Application.Users.Lock;

public sealed record LockUserCommand(Guid UserId, string? Reason) : ICommand;
