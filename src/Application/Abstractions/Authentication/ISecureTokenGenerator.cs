namespace Application.Abstractions.Authentication;

/// <summary>
/// Generates high-entropy random secrets (refresh tokens, API keys). The raw value is returned to the
/// caller once; only its hash (see <see cref="ITokenHasher"/>) is persisted.
/// </summary>
public interface ISecureTokenGenerator
{
    string Generate();
}
