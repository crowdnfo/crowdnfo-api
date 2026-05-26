namespace Domain.Products;

public sealed class Episode
{
    private Episode()
    {
    }

    public int Id { get; private set; }

    public int SeasonId { get; private set; }

    public int EpisodeNumber { get; private set; }

    public string Title { get; private set; }

    public string? ImdbId { get; private set; }

    public int? RuntimeSeconds { get; private set; }

    public string? Plot { get; private set; }

    public decimal? Rating { get; private set; }

    public int? VoteCount { get; private set; }

    public DateOnly? AirDate { get; private set; }

    public string? ImageUrl { get; private set; }

    internal static Episode Create(int episodeNumber, string title) =>
        new()
        {
            EpisodeNumber = episodeNumber,
            Title = title
        };

    public void SetMetadata(
        string? imdbId,
        int? runtimeSeconds,
        string? plot,
        decimal? rating,
        int? voteCount,
        DateOnly? airDate,
        string? imageUrl)
    {
        ImdbId = imdbId is null ? null : ImdbIdNormalizer.Normalize(imdbId);
        RuntimeSeconds = runtimeSeconds;
        Plot = plot;
        Rating = rating;
        VoteCount = voteCount;
        AirDate = airDate;
        ImageUrl = imageUrl;
    }
}
