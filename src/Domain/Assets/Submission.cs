using SharedKernel;

namespace Domain.Assets;

/// <summary>
/// One user's submission of an asset. <see cref="TrustGradeAtSubmit"/> is a snapshot, so later role
/// changes do not retro-edit past credits. Unique per (asset, user); optionally links to a variant.
/// </summary>
public sealed class Submission : IHasTimestamps
{
    private Submission()
    {
    }

    public Guid Id { get; private set; }

    public Guid AssetId { get; private set; }

    public Guid UserId { get; private set; }

    public int TrustGradeAtSubmit { get; private set; }

    public Guid? VariantId { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    internal static Submission Create(Guid assetId, Guid userId, int trustGradeAtSubmit, Guid? variantId) =>
        new()
        {
            Id = Guid.CreateVersion7(),
            AssetId = assetId,
            UserId = userId,
            TrustGradeAtSubmit = trustGradeAtSubmit,
            VariantId = variantId
        };
}
