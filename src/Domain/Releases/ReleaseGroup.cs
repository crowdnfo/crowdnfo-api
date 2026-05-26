namespace Domain.Releases;

public sealed class ReleaseGroup
{
    private ReleaseGroup()
    {
    }

    public ReleaseGroup(string name)
    {
        Name = name;
    }

    public int Id { get; private set; }

    public string Name { get; private set; }
}
