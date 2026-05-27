using Application.Abstractions.Messaging;

namespace Application.Users.Refresh;

public sealed record RefreshCommand(string RefreshToken) : ICommand<AuthResponse>;
