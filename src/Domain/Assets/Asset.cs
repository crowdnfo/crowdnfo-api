using Domain.Moderation;
using SharedKernel;

namespace Domain.Assets;

/// <summary>
/// Deduplicated content: one row per content hash. Mutated only through the release aggregate root,
/// hence the internal mutators.
/// </summary>
public abstract class Asset : IHasTimestamps
{
    private readonly List<Submission> _submissions = [];

    protected Asset()
    {
    }

    public Guid Id { get; private set; }

    public int ReleaseId { get; private set; }

    public abstract SubmissionType Type { get; }

    public string ContentHash { get; private set; }

    public ModerationState ModerationState { get; private set; }

    public int CumulativeTrustScore { get; private set; }

    public int SubmissionCount { get; private set; }

    public Guid OriginalSubmitterUserId { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyCollection<Submission> Submissions => _submissions.AsReadOnly();

    public bool IsVisible => ModerationState == ModerationState.Visible;

    protected void Initialize(string contentHash)
    {
        Id = Guid.CreateVersion7();
        ContentHash = contentHash;
        ModerationState = ModerationState.Visible;
    }

    /// <summary>Write-once: the first submitter is fixed because the highscore "First" counts credit them.</summary>
    internal void SetOriginalSubmitter(Guid userId)
    {
        if (OriginalSubmitterUserId == Guid.Empty)
        {
            OriginalSubmitterUserId = userId;
        }
    }

    internal Submission AddSubmission(Guid userId, int trustScoreAtSubmit, Guid? variantId)
    {
        var submission = Submission.Create(Id, userId, trustScoreAtSubmit, variantId);
        _submissions.Add(submission);
        return submission;
    }

    internal void Remove() => ModerationState = ModerationState.Removed;

    internal void Restore() => ModerationState = ModerationState.Visible;

    internal void SetRanking(int cumulativeTrustScore, int submissionCount)
    {
        CumulativeTrustScore = cumulativeTrustScore;
        SubmissionCount = submissionCount;
    }
}
