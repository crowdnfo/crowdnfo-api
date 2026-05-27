namespace Application.Abstractions.Authentication;

/// <summary>
/// Fast, deterministic hashing for high-entropy secrets (refresh tokens, API keys) so they can be
/// looked up by their hash. Deliberately not a slow password hash (bcrypt/argon2): these secrets
/// are high-entropy, so the slow-hash rationale does not apply and would break lookup.
/// </summary>
public interface ITokenHasher
{
    string Hash(string token);
}
