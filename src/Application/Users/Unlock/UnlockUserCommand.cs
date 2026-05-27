using Application.Abstractions.Messaging;

namespace Application.Users.Unlock;

public sealed record UnlockUserCommand(Guid UserId) : ICommand;
