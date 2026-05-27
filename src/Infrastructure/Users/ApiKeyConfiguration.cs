using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Users;

internal sealed class ApiKeyConfiguration : IEntityTypeConfiguration<ApiKey>
{
    public void Configure(EntityTypeBuilder<ApiKey> builder)
    {
        builder.HasKey(apiKey => apiKey.Id);
        builder.Property(apiKey => apiKey.Id).ValueGeneratedNever();
        builder.Property(apiKey => apiKey.Name).HasMaxLength(100);
        builder.Property(apiKey => apiKey.KeyHash).HasMaxLength(128);

        builder.HasIndex(apiKey => apiKey.KeyHash).IsUnique();
        builder.HasIndex(apiKey => new { apiKey.UserId, apiKey.Name }).IsUnique();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(apiKey => apiKey.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
