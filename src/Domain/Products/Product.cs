using SharedKernel;

namespace Domain.Products;

/// <summary>
/// The work a release represents. Base for <see cref="Movie"/> and <see cref="TvSeries"/>; future
/// types (Game, Book, ...) attach as new tables off this base. Holds the metadata common to all.
/// </summary>
public abstract class Product : Entity, IHasTimestamps
{
    private readonly List<ProductExternalId> _externalIds = [];
    private readonly List<AlternativeTitle> _alternativeTitles = [];
    private readonly List<ProductReleaseDate> _releaseDates = [];
    private readonly List<Genre> _genres = [];
    private readonly List<Country> _countries = [];

    protected Product()
    {
    }

    public int Id { get; private set; }

    public string Title { get; private set; }

    public int? Year { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyCollection<ProductExternalId> ExternalIds => _externalIds.AsReadOnly();

    public IReadOnlyCollection<AlternativeTitle> AlternativeTitles => _alternativeTitles.AsReadOnly();

    public IReadOnlyCollection<ProductReleaseDate> ReleaseDates => _releaseDates.AsReadOnly();

    public IReadOnlyCollection<Genre> Genres => _genres.AsReadOnly();

    public IReadOnlyCollection<Country> Countries => _countries.AsReadOnly();

    public string? GetExternalId(ExternalIdSource source) => _externalIds.Find(e => e.Source == source)?.Value;

    public void AddOrUpdateExternalId(ExternalIdSource source, string value)
    {
        ProductExternalId? existing = _externalIds.Find(e => e.Source == source);
        if (existing is not null)
        {
            existing.UpdateValue(value);
            return;
        }

        _externalIds.Add(ProductExternalId.Create(source, value));
    }

    public void AddGenre(Genre genre)
    {
        if (!_genres.Contains(genre))
        {
            _genres.Add(genre);
        }
    }

    public void AddCountry(Country country)
    {
        if (!_countries.Contains(country))
        {
            _countries.Add(country);
        }
    }

    public void AddAlternativeTitle(AlternativeTitle alternativeTitle)
    {
        if (!_alternativeTitles.Contains(alternativeTitle))
        {
            _alternativeTitles.Add(alternativeTitle);
        }
    }

    public void AddReleaseDate(ProductReleaseDate releaseDate)
    {
        if (!_releaseDates.Contains(releaseDate))
        {
            _releaseDates.Add(releaseDate);
        }
    }

    protected void SetCore(string title, int? year)
    {
        Title = title;
        Year = year;
    }
}
