namespace Domain.Products;

public sealed class Season
{
    private readonly List<Episode> _episodes = [];

    private Season()
    {
    }

    public int Id { get; private set; }

    public int TvSeriesId { get; private set; }

    public int SeasonNumber { get; private set; }

    public int? AiredYear { get; private set; }

    public IReadOnlyCollection<Episode> Episodes => _episodes.AsReadOnly();

    internal static Season Create(int seasonNumber) =>
        new()
        {
            SeasonNumber = seasonNumber
        };

    public Episode GetOrAddEpisode(int episodeNumber, string title)
    {
        Episode? existing = _episodes.Find(e => e.EpisodeNumber == episodeNumber);
        if (existing is not null)
        {
            return existing;
        }

        var episode = Episode.Create(episodeNumber, title);
        _episodes.Add(episode);
        return episode;
    }

    public void SetAiredYear(int? airedYear) => AiredYear = airedYear;
}
