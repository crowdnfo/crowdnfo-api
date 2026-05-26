using SharedKernel;

namespace Domain.Releases;

/// <summary>Raised when a release is linked to a product; the trigger for product-enrichment steps.</summary>
public sealed record ProductLinkedDomainEvent(int ReleaseId, int ProductId) : IDomainEvent;
