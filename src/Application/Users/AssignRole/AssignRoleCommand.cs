using Application.Abstractions.Messaging;

namespace Application.Users.AssignRole;

public sealed record AssignRoleCommand(Guid UserId, int RoleId) : ICommand;
