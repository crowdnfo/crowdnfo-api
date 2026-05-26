namespace Domain.Releases;

/// <summary>An alternative name; unique per release.</summary>
public sealed class ReleaseAlias
{
    private ReleaseAlias()
    {
    }

    internal ReleaseAlias(string aliasName)
    {
        AliasName = aliasName;
    }

    public int Id { get; private set; }

    public int ReleaseId { get; private set; }

    public string AliasName { get; private set; }
}
