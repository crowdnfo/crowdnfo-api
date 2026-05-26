using SharedKernel;

namespace Domain.Users;

public sealed class ApiKey : IHasTimestamps
{
    private ApiKey()
    {
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public string Name { get; private set; }

    public string KeyHash { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public static ApiKey Create(Guid userId, string name, string keyHash) =>
        new()
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            Name = name,
            KeyHash = keyHash
        };
}
