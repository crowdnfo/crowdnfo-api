using SharedKernel;

namespace Domain.Highscores;

/// <summary>
/// Denormalized counters, recomputed from source rather than incremented in place. The "First"
/// counts credit the original submitter of a piece of content.
/// </summary>
public sealed class Highscore : IHasTimestamps
{
    private Highscore()
    {
    }

    public Guid UserId { get; private set; }

    public int NfoTotal { get; private set; }

    public int NfoFirst { get; private set; }

    public int MediaInfoTotal { get; private set; }

    public int MediaInfoFirst { get; private set; }

    public int FileListTotal { get; private set; }

    public int FileListFirst { get; private set; }

    public int CreatedReleases { get; private set; }

    public int Comments { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public static Highscore For(Guid userId) =>
        new()
        {
            UserId = userId
        };

    public void SetCounts(
        int nfoTotal,
        int nfoFirst,
        int mediaInfoTotal,
        int mediaInfoFirst,
        int fileListTotal,
        int fileListFirst,
        int createdReleases,
        int comments)
    {
        NfoTotal = nfoTotal;
        NfoFirst = nfoFirst;
        MediaInfoTotal = mediaInfoTotal;
        MediaInfoFirst = mediaInfoFirst;
        FileListTotal = fileListTotal;
        FileListFirst = fileListFirst;
        CreatedReleases = createdReleases;
        Comments = comments;
    }
}
