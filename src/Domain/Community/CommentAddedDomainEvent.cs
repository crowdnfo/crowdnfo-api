using SharedKernel;

namespace Domain.Community;

public sealed record CommentAddedDomainEvent(int ReleaseId, Guid UserId) : IDomainEvent;
