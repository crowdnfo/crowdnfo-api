using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.Login;

internal sealed class LoginCommandHandler(
    IApplicationDbContext context,
    IPasswordHasher passwordHasher,
    ITokenProvider tokenProvider,
    ITokenHasher tokenHasher,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<LoginCommand, AuthResponse>
{
    // Verified against on the no-user path so response time can't reveal whether the username exists.
    // Must stay in PasswordHasher's "{hashHex}-{saltHex}" shape so Verify can parse it.
    private const string DummyPasswordHash =
        "0F1E2D3C4B5A69788796A5B4C3D2E1F00F1E2D3C4B5A69788796A5B4C3D2E1F0-0F1E2D3C4B5A69788796A5B4C3D2E1F0";

    public async Task<Result<AuthResponse>> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        string normalizedUsername = User.NormalizeUsername(command.Username);

        User? user = await context.Users
            .FirstOrDefaultAsync(u => u.NormalizedUsername == normalizedUsername, cancellationToken);

        if (user is null)
        {
            passwordHasher.Verify(command.Password, DummyPasswordHash);

            return Result.Failure<AuthResponse>(UserErrors.InvalidCredentials);
        }

        if (!passwordHasher.Verify(command.Password, user.PasswordHash))
        {
            user.RecordFailedLogin();
            await context.SaveChangesAsync(cancellationToken);

            return Result.Failure<AuthResponse>(UserErrors.InvalidCredentials);
        }

        Result canSignIn = user.EnsureCanSignIn();
        if (canSignIn.IsFailure)
        {
            return Result.Failure<AuthResponse>(canSignIn.Error);
        }

        Role? role = await context.Roles
            .FirstOrDefaultAsync(r => r.Id == user.RoleId, cancellationToken);

        if (role is null)
        {
            return Result.Failure<AuthResponse>(RoleErrors.NotFound(user.RoleId));
        }

        TokenPair tokens = tokenProvider.IssueTokens(user, role);

        context.RefreshTokens.Add(RefreshToken.Issue(
            user.Id,
            tokenHasher.Hash(tokens.RefreshToken),
            tokens.RefreshTokenExpiresAtUtc));

        user.RecordSuccessfulLogin(dateTimeProvider.UtcNow);

        await context.SaveChangesAsync(cancellationToken);

        return new AuthResponse(
            tokens.AccessToken,
            tokens.RefreshToken,
            tokens.ExpiresInSeconds,
            new AuthUser(user.Id, user.Username, user.Email, role.Name, user.ProfileVisibility));
    }
}
