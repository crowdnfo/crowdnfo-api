using Domain.Moderation;
using SharedKernel;

namespace Domain.Community;

public sealed class Comment : Entity, IHasTimestamps
{
    private Comment()
    {
    }

    public int Id { get; private set; }

    public int ReleaseId { get; private set; }

    public Guid? UserId { get; private set; }

    public string Text { get; private set; }

    public bool IsEdited { get; private set; }

    public ModerationState ModerationState { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public static Comment Create(int releaseId, Guid userId, string text)
    {
        var comment = new Comment
        {
            ReleaseId = releaseId,
            UserId = userId,
            Text = text,
            ModerationState = ModerationState.Visible
        };

        comment.Raise(new CommentAddedDomainEvent(releaseId, userId));

        return comment;
    }

    public void Edit(string text)
    {
        Text = text;
        IsEdited = true;
    }

    public void Remove()
    {
        if (ModerationState == ModerationState.Removed)
        {
            return;
        }

        ModerationState = ModerationState.Removed;

        if (UserId is not null)
        {
            Raise(new CommentRemovedDomainEvent(ReleaseId, UserId.Value));
        }
    }

    public void Restore() => ModerationState = ModerationState.Visible;
}
