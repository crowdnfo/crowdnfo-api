using SharedKernel;

namespace Domain.Community;

/// <summary>
/// A user report against exactly one target: a submission OR a comment (never both). The two
/// factories enforce that invariant; the unused target id stays null.
/// </summary>
public sealed class Report : IHasTimestamps
{
    private Report()
    {
    }

    public int Id { get; private set; }

    public Guid? SubmissionId { get; private set; }

    public int? CommentId { get; private set; }

    public Guid? ReportedByUserId { get; private set; }

    public string Reason { get; private set; }

    public ReportStatus Status { get; private set; }

    public Guid? ReviewedByUserId { get; private set; }

    public DateTime? ReviewedAt { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public static Report ForSubmission(Guid submissionId, Guid reportedByUserId, string reason) =>
        new()
        {
            SubmissionId = submissionId,
            ReportedByUserId = reportedByUserId,
            Reason = reason,
            Status = ReportStatus.Open
        };

    public static Report ForComment(int commentId, Guid reportedByUserId, string reason) =>
        new()
        {
            CommentId = commentId,
            ReportedByUserId = reportedByUserId,
            Reason = reason,
            Status = ReportStatus.Open
        };

    public Result Resolve(Guid reviewedByUserId, DateTime reviewedAt)
    {
        if (Status == ReportStatus.Resolved)
        {
            return Result.Failure(ReportErrors.AlreadyResolved);
        }

        Status = ReportStatus.Resolved;
        ReviewedByUserId = reviewedByUserId;
        ReviewedAt = reviewedAt;

        return Result.Success();
    }
}
