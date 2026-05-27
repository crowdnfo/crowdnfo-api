using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Application.Abstractions.Data;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }

    DbSet<Role> Roles { get; }

    DbSet<RefreshToken> RefreshTokens { get; }

    DbSet<ApiKey> ApiKeys { get; }

    DbSet<UserApplication> UserApplications { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
