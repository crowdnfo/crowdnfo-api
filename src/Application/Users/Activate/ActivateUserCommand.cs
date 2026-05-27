using Application.Abstractions.Messaging;

namespace Application.Users.Activate;

public sealed record ActivateUserCommand(Guid UserId) : ICommand;
