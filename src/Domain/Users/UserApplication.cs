using SharedKernel;

namespace Domain.Users;

/// <summary>
/// <see cref="UserId"/> is nullable because a rejected applicant's inactive user is deleted while
/// this record is kept as history. <see cref="ApplicationData"/> is an opaque JSON blob of form answers.
/// </summary>
public sealed class UserApplication : IHasTimestamps
{
    private UserApplication()
    {
    }

    public int Id { get; private set; }

    public Guid? UserId { get; private set; }

    public string ApplicationData { get; private set; }

    public ApplicationStatus Status { get; private set; }

    public Guid? ReviewedByUserId { get; private set; }

    public string? ReviewComment { get; private set; }

    public DateTime? ReviewedAt { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public static UserApplication Submit(Guid userId, string applicationData) =>
        new()
        {
            UserId = userId,
            ApplicationData = applicationData,
            Status = ApplicationStatus.Pending
        };

    public Result Approve(Guid reviewedByUserId, DateTime reviewedAt, string? comment = null) =>
        Review(ApplicationStatus.Approved, reviewedByUserId, reviewedAt, comment);

    public Result Reject(Guid reviewedByUserId, DateTime reviewedAt, string? comment = null) =>
        Review(ApplicationStatus.Rejected, reviewedByUserId, reviewedAt, comment);

    private Result Review(ApplicationStatus status, Guid reviewedByUserId, DateTime reviewedAt, string? comment)
    {
        if (Status != ApplicationStatus.Pending)
        {
            return Result.Failure(UserApplicationErrors.NotPending);
        }

        Status = status;
        ReviewedByUserId = reviewedByUserId;
        ReviewedAt = reviewedAt;
        ReviewComment = comment;

        return Result.Success();
    }
}
