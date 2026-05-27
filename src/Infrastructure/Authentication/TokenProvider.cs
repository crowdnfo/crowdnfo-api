using System.Security.Claims;
using System.Text;
using Application.Abstractions.Authentication;
using Domain.Users;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using SharedKernel;

namespace Infrastructure.Authentication;

internal sealed class TokenProvider(
    IOptions<JwtOptions> options,
    IDateTimeProvider dateTimeProvider,
    ISecureTokenGenerator tokenGenerator) : ITokenProvider
{
    private readonly JwtOptions _options = options.Value;

    public TokenPair IssueTokens(User user, Role role)
    {
        DateTime now = dateTimeProvider.UtcNow;

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(TokenClaimTypes.Username, user.Username),
            new(TokenClaimTypes.Role, role.Name)
        };

        if (role.HasAdminAccess)
        {
            claims.Add(new Claim(TokenClaimTypes.Admin, "true"));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = now.AddMinutes(_options.AccessTokenExpirationMinutes),
            SigningCredentials = credentials,
            Issuer = _options.Issuer,
            Audience = _options.Audience
        };

        var handler = new JsonWebTokenHandler();
        string accessToken = handler.CreateToken(tokenDescriptor);

        return new TokenPair(
            accessToken,
            _options.AccessTokenExpirationMinutes * 60,
            tokenGenerator.Generate(),
            now.AddDays(_options.RefreshTokenExpirationDays));
    }
}
