namespace Domain.Products;

public sealed class Movie : Product
{
    private readonly List<string> _spokenLanguages = [];

    private Movie()
    {
    }

    public int? Runtime { get; private set; }

    public decimal? ImdbRating { get; private set; }

    public int? ImdbVotes { get; private set; }

    public string? Plot { get; private set; }

    public string? OriginalLanguage { get; private set; }

    public IReadOnlyCollection<string> SpokenLanguages => _spokenLanguages.AsReadOnly();

    public string? ImdbId => GetExternalId(ExternalIdSource.Imdb);

    public static Movie Create(string title, int? year)
    {
        var movie = new Movie();
        movie.SetCore(title, year);
        return movie;
    }

    public void SetMetadata(
        int? runtime,
        decimal? imdbRating,
        int? imdbVotes,
        string? plot,
        string? originalLanguage,
        IReadOnlyList<string> spokenLanguages)
    {
        Runtime = runtime;
        ImdbRating = imdbRating;
        ImdbVotes = imdbVotes;
        Plot = plot;
        OriginalLanguage = originalLanguage;
        _spokenLanguages.Clear();
        _spokenLanguages.AddRange(spokenLanguages);
    }
}
