using SharedKernel;

namespace Domain.Users;

public sealed record UserActivatedDomainEvent(Guid UserId) : IDomainEvent;
