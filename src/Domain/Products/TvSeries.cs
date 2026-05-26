namespace Domain.Products;

public sealed class TvSeries : Product
{
    private readonly List<Season> _seasons = [];
    private readonly List<string> _spokenLanguages = [];

    private TvSeries()
    {
    }

    public SeriesStatus Status { get; private set; }

    public DateOnly? FirstAirDate { get; private set; }

    public DateOnly? LastAirDate { get; private set; }

    public int? EndYear { get; private set; }

    public decimal? ImdbRating { get; private set; }

    public int? ImdbVotes { get; private set; }

    public string? Plot { get; private set; }

    public string? OriginalLanguage { get; private set; }

    public IReadOnlyCollection<string> SpokenLanguages => _spokenLanguages.AsReadOnly();

    public string? ImdbId => GetExternalId(ExternalIdSource.Imdb);

    public string? TvdbId => GetExternalId(ExternalIdSource.Tvdb);

    public IReadOnlyCollection<Season> Seasons => _seasons.AsReadOnly();

    public static TvSeries Create(string title, int? year)
    {
        var series = new TvSeries
        {
            Status = SeriesStatus.Unknown
        };
        series.SetCore(title, year);
        return series;
    }

    public Season GetOrAddSeason(int seasonNumber)
    {
        Season? existing = _seasons.Find(s => s.SeasonNumber == seasonNumber);
        if (existing is not null)
        {
            return existing;
        }

        var season = Season.Create(seasonNumber);
        _seasons.Add(season);
        return season;
    }

    public void SetMetadata(
        SeriesStatus status,
        DateOnly? firstAirDate,
        DateOnly? lastAirDate,
        int? endYear,
        decimal? imdbRating,
        int? imdbVotes,
        string? plot,
        string? originalLanguage,
        IReadOnlyList<string> spokenLanguages)
    {
        Status = status;
        FirstAirDate = firstAirDate;
        LastAirDate = lastAirDate;
        EndYear = endYear;
        ImdbRating = imdbRating;
        ImdbVotes = imdbVotes;
        Plot = plot;
        OriginalLanguage = originalLanguage;
        _spokenLanguages.Clear();
        _spokenLanguages.AddRange(spokenLanguages);
    }
}
