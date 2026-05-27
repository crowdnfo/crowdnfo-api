using Application.Abstractions.Messaging;
using Domain.Users;

namespace Application.Users.UpdateProfile;

public sealed record UpdateProfileCommand(string Email, ProfileVisibility Visibility) : ICommand;
