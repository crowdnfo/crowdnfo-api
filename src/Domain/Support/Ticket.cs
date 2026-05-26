using SharedKernel;

namespace Domain.Support;

public sealed class Ticket : IHasTimestamps
{
    private Ticket()
    {
    }

    public int Id { get; private set; }

    public Guid? UserId { get; private set; }

    public string Subject { get; private set; }

    public string Message { get; private set; }

    public TicketStatus Status { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public static Ticket Open(Guid userId, string subject, string message) =>
        new()
        {
            UserId = userId,
            Subject = subject,
            Message = message,
            Status = TicketStatus.Open
        };

    public void Close() => Status = TicketStatus.Closed;
}
