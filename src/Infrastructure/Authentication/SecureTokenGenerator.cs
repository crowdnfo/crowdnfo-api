using System.Security.Cryptography;
using Application.Abstractions.Authentication;

namespace Infrastructure.Authentication;

internal sealed class SecureTokenGenerator : ISecureTokenGenerator
{
    private const int TokenSizeInBytes = 32;

    public string Generate() => Convert.ToHexString(RandomNumberGenerator.GetBytes(TokenSizeInBytes));
}
