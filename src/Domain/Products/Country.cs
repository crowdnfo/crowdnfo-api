namespace Domain.Products;

/// <summary><see cref="Code"/> is ISO 3166-1.</summary>
public sealed class Country
{
    private Country()
    {
    }

    public Country(string code, string name)
    {
        Code = code;
        Name = name;
    }

    public int Id { get; private set; }

    public string Code { get; private set; }

    public string Name { get; private set; }
}
