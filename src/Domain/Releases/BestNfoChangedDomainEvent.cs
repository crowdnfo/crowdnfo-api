using SharedKernel;

namespace Domain.Releases;

/// <summary>
/// Raised when a release's best (canonical) NFO changes; the trigger for NFO-derived processing
/// steps. <see cref="NfoAssetId"/> and <see cref="ContentHash"/> are null when the best NFO was
/// removed with no replacement.
/// </summary>
public sealed record BestNfoChangedDomainEvent(int ReleaseId, Guid? NfoAssetId, string? ContentHash) : IDomainEvent;
