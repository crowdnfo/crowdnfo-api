namespace Domain.Users;

/// <summary>
/// Canonical role names. Trust scores and the admin flag are data (see <see cref="Role"/>);
/// only the names are referenced from code (e.g. resolving the default role for new members).
/// </summary>
public static class RoleNames
{
    public const string Admin = "Admin";
    public const string TrustedBot = "TrustedBot";
    public const string Vip = "VIP";
    public const string Trusted = "Trusted";
    public const string User = "User";

    public const string Default = User;
}
