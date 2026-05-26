namespace Domain.Users;

/// <summary>
/// Trust score is a ranking signal for submissions. Roles are seeded and configurable, not
/// user-created.
/// </summary>
public sealed class Role
{
    private Role()
    {
    }

    public Role(string name, int trustScore, bool hasAdminAccess)
    {
        Name = name;
        TrustScore = trustScore;
        HasAdminAccess = hasAdminAccess;
    }

    public int Id { get; private set; }

    public string Name { get; private set; }

    public int TrustScore { get; private set; }

    public bool HasAdminAccess { get; private set; }
}
