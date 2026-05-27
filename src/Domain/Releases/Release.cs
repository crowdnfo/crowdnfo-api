using Domain.Assets;
using Domain.Tags;
using SharedKernel;

namespace Domain.Releases;

/// <summary>
/// Aggregate root for the submission graph. All writes (submit, remove, restore) flow through the
/// root, which recomputes trust rankings and re-selects the canonical variant on every change.
/// </summary>
public sealed class Release : Entity, IHasTimestamps
{
    private readonly List<ReleaseAlias> _aliases = [];
    private readonly List<ReleaseVariant> _variants = [];
    private readonly List<Asset> _assets = [];
    private readonly List<ReleaseTag> _nameTags = [];

    private Release()
    {
    }

    public int Id { get; private set; }

    public string CanonicalName { get; private set; }

    public ReleaseCategory Category { get; private set; }

    public int? ReleaseGroupId { get; private set; }

    public int? ProductId { get; private set; }

    /// <summary>The hash of the top-ranked variant (a distinct "canonical" choice from the name).</summary>
    public string? CanonicalFileHash { get; private set; }

    public Guid? CreatedByUserId { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyCollection<ReleaseAlias> Aliases => _aliases.AsReadOnly();

    public IReadOnlyCollection<ReleaseVariant> Variants => _variants.AsReadOnly();

    public IReadOnlyCollection<Asset> Assets => _assets.AsReadOnly();

    public IReadOnlyCollection<ReleaseTag> NameTags => _nameTags.AsReadOnly();

    public static Release Create(
        string canonicalName,
        ReleaseCategory category,
        Guid? createdByUserId,
        int? releaseGroupId = null) =>
        new()
        {
            CanonicalName = canonicalName,
            Category = category,
            CreatedByUserId = createdByUserId,
            ReleaseGroupId = releaseGroupId
        };

    public void AddAlias(string aliasName)
    {
        bool alreadyKnown = aliasName == CanonicalName || _aliases.Exists(a => a.AliasName == aliasName);
        if (alreadyKnown)
        {
            return;
        }

        _aliases.Add(new ReleaseAlias(aliasName));
    }

    /// <summary>Idempotent per user; the asset content is already deduplicated by the caller.</summary>
    public Submission SubmitAsset(Asset asset, Guid userId, int trustScoreAtSubmit, string? variantFileHash = null)
    {
        Submission? existing = _assets.Contains(asset)
            ? asset.Submissions.FirstOrDefault(s => s.UserId == userId)
            : null;
        if (existing is not null)
        {
            return existing;
        }

        string? previousBestNfoHash = BestNfo()?.ContentHash;

        if (!_assets.Contains(asset))
        {
            asset.SetOriginalSubmitter(userId);
            _assets.Add(asset);
        }

        Guid? variantId = null;
        if (variantFileHash is not null)
        {
            ReleaseVariant variant = GetOrAddVariant(variantFileHash);
            variantId = variant.Id;
        }

        Submission submission = asset.AddSubmission(userId, trustScoreAtSubmit, variantId);
        RecalculateRankings(previousBestNfoHash);
        return submission;
    }

    public Result RemoveAsset(Guid assetId)
    {
        Asset? asset = _assets.Find(a => a.Id == assetId);
        if (asset is null)
        {
            return Result.Failure(SubmissionErrors.AssetNotFound(assetId));
        }

        string? previousBestNfoHash = BestNfo()?.ContentHash;
        asset.Remove();
        RecalculateRankings(previousBestNfoHash);
        return Result.Success();
    }

    public Result RestoreAsset(Guid assetId)
    {
        Asset? asset = _assets.Find(a => a.Id == assetId);
        if (asset is null)
        {
            return Result.Failure(SubmissionErrors.AssetNotFound(assetId));
        }

        string? previousBestNfoHash = BestNfo()?.ContentHash;
        asset.Restore();
        RecalculateRankings(previousBestNfoHash);
        return Result.Success();
    }

    public Result LinkProduct(int productId)
    {
        if (ProductId == productId)
        {
            return Result.Success();
        }

        EnsurePersisted();
        ProductId = productId;
        Raise(new ProductLinkedDomainEvent(Id, productId));
        return Result.Success();
    }

    public void SetNameTags(IEnumerable<Tag> tags)
    {
        _nameTags.Clear();
        foreach (Tag tag in tags)
        {
            _nameTags.Add(new ReleaseTag(tag));
        }
    }

    private ReleaseVariant GetOrAddVariant(string fileHash)
    {
        ReleaseVariant? existing = _variants.Find(v => v.FileHash == fileHash);
        if (existing is not null)
        {
            return existing;
        }

        var variant = ReleaseVariant.Create(fileHash);
        _variants.Add(variant);
        return variant;
    }

    /// <summary>
    /// Content-mutating behavior must run on a persisted release: the id-carrying domain events
    /// snapshot the store-generated int <see cref="Id"/> at raise time. A violation is a wiring bug,
    /// hence a throw rather than a <see cref="Result"/>.
    /// </summary>
    private void EnsurePersisted()
    {
        if (Id == 0)
        {
            throw new InvalidOperationException(
                "Content cannot be submitted to a release before it is persisted; its identity " +
                "(carried by domain events) is not yet assigned. Persist the release first.");
        }
    }

    /// <summary>
    /// Recomputes cumulative trust scores from the distinct submitters' snapshot scores (removed
    /// assets do not count toward variant scores) and re-selects the canonical variant.
    /// </summary>
    private void RecalculateRankings(string? previousBestNfoHash)
    {
        EnsurePersisted();

        foreach (Asset asset in _assets)
        {
            List<int> scores = DistinctUserScores(asset.Submissions);
            asset.SetRanking(scores.Sum(), scores.Count);
        }

        foreach (ReleaseVariant variant in _variants)
        {
            IEnumerable<Submission> variantSubmissions = _assets
                .Where(a => a.IsVisible)
                .SelectMany(a => a.Submissions)
                .Where(s => s.VariantId == variant.Id);

            List<int> scores = DistinctUserScores(variantSubmissions);
            variant.SetRanking(scores.Sum(), scores.Count);
        }

        ReleaseVariant? topVariant = _variants
            .OrderByDescending(v => v.CumulativeTrustScore)
            .ThenByDescending(v => v.SubmissionCount)
            .ThenBy(v => v.CreatedAt)
            .ThenBy(v => v.Id)
            .FirstOrDefault();

        CanonicalFileHash = topVariant?.FileHash;

        NfoAsset? bestNfoAfter = BestNfo();
        if (bestNfoAfter?.ContentHash != previousBestNfoHash)
        {
            Raise(new BestNfoChangedDomainEvent(Id, bestNfoAfter?.Id, bestNfoAfter?.ContentHash));
        }
    }

    private NfoAsset? BestNfo() =>
        _assets
            .OfType<NfoAsset>()
            .Where(a => a.IsVisible)
            .OrderByDescending(a => a.CumulativeTrustScore)
            .ThenByDescending(a => a.SubmissionCount)
            .ThenBy(a => a.CreatedAt)
            .ThenBy(a => a.Id)
            .FirstOrDefault();

    private static List<int> DistinctUserScores(IEnumerable<Submission> submissions) =>
        submissions
            .GroupBy(s => s.UserId)
            .Select(g => g.Max(s => s.TrustScoreAtSubmit))
            .ToList();
}
