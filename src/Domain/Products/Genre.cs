namespace Domain.Products;

public sealed class Genre
{
    private Genre()
    {
    }

    public Genre(string name)
    {
        Name = name;
    }

    public int Id { get; private set; }

    public string Name { get; private set; }
}
