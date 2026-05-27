using System.Security.Claims;
using System.Text.Encodings.Web;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Domain.Users;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Infrastructure.Authentication;

internal sealed class ApiKeyAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(ApiKeyAuthenticationDefaults.HeaderName, out StringValues headerValues))
        {
            return AuthenticateResult.NoResult();
        }

        string presentedKey = headerValues.ToString();

        if (string.IsNullOrWhiteSpace(presentedKey))
        {
            return AuthenticateResult.NoResult();
        }

        ITokenHasher tokenHasher = Context.RequestServices.GetRequiredService<ITokenHasher>();
        IApplicationDbContext dbContext = Context.RequestServices.GetRequiredService<IApplicationDbContext>();

        string keyHash = tokenHasher.Hash(presentedKey);

        Guid? owner = await dbContext.ApiKeys
            .Where(apiKey => apiKey.KeyHash == keyHash)
            .Select(apiKey => (Guid?)apiKey.UserId)
            .FirstOrDefaultAsync();

        if (owner is null)
        {
            return AuthenticateResult.Fail("Invalid API key");
        }

        User? user = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(account => account.Id == owner);

        if (user is null || user.EnsureCanSignIn().IsFailure)
        {
            return AuthenticateResult.Fail("The API key cannot be used");
        }

        string? roleName = await dbContext.Roles
            .Where(role => role.Id == user.RoleId)
            .Select(role => role.Name)
            .FirstOrDefaultAsync();

        Claim[] claims =
        [
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(TokenClaimTypes.Username, user.Username),
            new Claim(TokenClaimTypes.Role, roleName ?? string.Empty)
        ];

        var identity = new ClaimsIdentity(claims, Scheme.Name, TokenClaimTypes.Username, TokenClaimTypes.Role);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}
