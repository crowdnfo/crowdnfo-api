using SharedKernel;

namespace Domain.Releases;

/// <summary>
/// A distinct physical file under the same release name, keyed by the SHA-256 hash of its main
/// payload. Ranks independently by its own trust grade; the top-ranked variant's hash becomes the
/// release's canonical file hash. Part of the <see cref="Release"/> aggregate.
/// </summary>
public sealed class ReleaseVariant : IHasTimestamps
{
    private ReleaseVariant()
    {
    }

    public Guid Id { get; private set; }

    public int ReleaseId { get; private set; }

    public string FileHash { get; private set; }

    public int CumulativeTrustGrade { get; private set; }

    public int SubmissionCount { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    internal static ReleaseVariant Create(string fileHash) =>
        new()
        {
            Id = Guid.CreateVersion7(),
            FileHash = fileHash
        };

    internal void SetRanking(int cumulativeTrustGrade, int submissionCount)
    {
        CumulativeTrustGrade = cumulativeTrustGrade;
        SubmissionCount = submissionCount;
    }
}
