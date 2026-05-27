using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Users;

internal sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasKey(role => role.Id);
        builder.Property(role => role.Name).HasMaxLength(50);
        builder.HasIndex(role => role.Name).IsUnique();

        builder.HasData(
            new { Id = 1, Name = RoleNames.Admin, TrustScore = 100, HasAdminAccess = true },
            new { Id = 2, Name = RoleNames.TrustedBot, TrustScore = 100, HasAdminAccess = false },
            new { Id = 3, Name = RoleNames.Vip, TrustScore = 50, HasAdminAccess = false },
            new { Id = 4, Name = RoleNames.Trusted, TrustScore = 40, HasAdminAccess = false },
            new { Id = 5, Name = RoleNames.User, TrustScore = 15, HasAdminAccess = false });
    }
}
