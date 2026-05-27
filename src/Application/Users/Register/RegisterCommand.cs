using Application.Abstractions.Messaging;
using Domain.Users;

namespace Application.Users.Register;

public sealed record RegisterCommand(
    string Username,
    string Email,
    string Password,
    string ApplicationData) : ICommand<RegisterResponse>;

public sealed record RegisterResponse(int Id, ApplicationStatus Status);
