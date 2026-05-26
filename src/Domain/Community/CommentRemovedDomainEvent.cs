using SharedKernel;

namespace Domain.Community;

public sealed record CommentRemovedDomainEvent(int ReleaseId, Guid UserId) : IDomainEvent;
