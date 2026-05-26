using SharedKernel;

namespace Domain.Users;

public sealed class User : Entity, IHasTimestamps
{
    private User()
    {
    }

    public Guid Id { get; private set; }

    public string Username { get; private set; }

    public string Email { get; private set; }

    public string PasswordHash { get; private set; }

    public int RoleId { get; private set; }

    public ProfileVisibility ProfileVisibility { get; private set; }

    public bool IsActivated { get; private set; }

    public bool IsLocked { get; private set; }

    public DateTime? LastLoginAt { get; private set; }

    public int FailedLoginAttempts { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    /// <summary>Creates a new, inactive user (activation is a separate admin-approved step).</summary>
    public static User Register(string username, string email, string passwordHash, int roleId)
    {
        var user = new User
        {
            Id = Guid.CreateVersion7(),
            Username = username,
            Email = email,
            PasswordHash = passwordHash,
            RoleId = roleId,
            ProfileVisibility = ProfileVisibility.Public,
            IsActivated = false,
            IsLocked = false,
            FailedLoginAttempts = 0
        };

        user.Raise(new UserRegisteredDomainEvent(user.Id));

        return user;
    }

    public Result Activate()
    {
        if (IsActivated)
        {
            return Result.Failure(UserErrors.AlreadyActivated);
        }

        IsActivated = true;
        Raise(new UserActivatedDomainEvent(Id));

        return Result.Success();
    }

    public void Lock() => IsLocked = true;

    public void Unlock()
    {
        IsLocked = false;
        FailedLoginAttempts = 0;
    }

    public void ChangeVisibility(ProfileVisibility visibility) => ProfileVisibility = visibility;

    public void RecordSuccessfulLogin(DateTime occurredAt)
    {
        LastLoginAt = occurredAt;
        FailedLoginAttempts = 0;
    }

    public void RecordFailedLogin() => FailedLoginAttempts++;
}
