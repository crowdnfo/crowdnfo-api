namespace Domain.Users;

/// <summary>
/// Trust weight is a ranking signal for submissions. Roles are seeded and configurable, not
/// user-created.
/// </summary>
public sealed class Role
{
    private Role()
    {
    }

    public Role(string name, int trustWeight, bool hasAdminAccess)
    {
        Name = name;
        TrustWeight = trustWeight;
        HasAdminAccess = hasAdminAccess;
    }

    public int Id { get; private set; }

    public string Name { get; private set; }

    public int TrustWeight { get; private set; }

    public bool HasAdminAccess { get; private set; }
}
