using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.Register;

internal sealed class RegisterCommandHandler(
    IApplicationDbContext context,
    IPasswordHasher passwordHasher) : ICommandHandler<RegisterCommand, RegisterResponse>
{
    public async Task<Result<RegisterResponse>> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        string normalizedEmail = User.NormalizeEmail(command.Email);
        string normalizedUsername = User.NormalizeUsername(command.Username);

        if (await context.Users.AnyAsync(u => u.NormalizedEmail == normalizedEmail, cancellationToken))
        {
            return Result.Failure<RegisterResponse>(UserErrors.EmailNotUnique);
        }

        if (await context.Users.AnyAsync(u => u.NormalizedUsername == normalizedUsername, cancellationToken))
        {
            return Result.Failure<RegisterResponse>(UserErrors.UsernameNotUnique);
        }

        Role? defaultRole = await context.Roles
            .FirstOrDefaultAsync(role => role.Name == RoleNames.Default, cancellationToken);

        if (defaultRole is null)
        {
            return Result.Failure<RegisterResponse>(RoleErrors.NotFoundByName(RoleNames.Default));
        }

        string passwordHash = passwordHasher.Hash(command.Password);

        var user = User.Register(command.Username, command.Email, passwordHash, defaultRole.Id);
        var application = UserApplication.Submit(user.Id, command.ApplicationData);

        context.Users.Add(user);
        context.UserApplications.Add(application);

        await context.SaveChangesAsync(cancellationToken);

        return new RegisterResponse(application.Id, application.Status);
    }
}
