using SharedKernel;

namespace Domain.Processing;

/// <summary>
/// Per-step processing status for a product (one row per product + step). Mirrors
/// <see cref="ReleaseProcessingStatus"/>; the two are deliberately separate so each carries a
/// typed owner FK and maps to its own table.
/// </summary>
public sealed class ProductProcessingStatus : IHasTimestamps
{
    private ProductProcessingStatus()
    {
    }

    public Guid Id { get; private set; }

    public int ProductId { get; private set; }

    public ProcessingStep Step { get; private set; }

    public ProcessingState State { get; private set; }

    public string? ProcessedInputHash { get; private set; }

    public DateTime? NextAttemptAt { get; private set; }

    public DateTime? LastAttemptedAt { get; private set; }

    public DateTime? LastSuccessfulAt { get; private set; }

    public string? ErrorMessage { get; private set; }

    public int RetryCount { get; private set; }

    public string? ResultData { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public static ProductProcessingStatus Create(int productId, ProcessingStep step) =>
        new()
        {
            Id = Guid.CreateVersion7(),
            ProductId = productId,
            Step = step,
            State = ProcessingState.Ready
        };

    public void MarkReady()
    {
        State = ProcessingState.Ready;
        NextAttemptAt = null;
    }

    public void MarkInProgress(DateTime now)
    {
        State = ProcessingState.InProgress;
        LastAttemptedAt = now;
    }

    public void MarkCompleted(DateTime now, string processedInputHash, string? resultData = null)
    {
        State = ProcessingState.Completed;
        LastSuccessfulAt = now;
        ProcessedInputHash = processedInputHash;
        ResultData = resultData;
        ErrorMessage = null;
        NextAttemptAt = null;
    }

    public void MarkNotApplicable() => State = ProcessingState.NotApplicable;

    public void MarkFailed(string error)
    {
        State = ProcessingState.Failed;
        ErrorMessage = error;
    }

    public void ScheduleRetry(DateTime nextAttemptAt, string? error = null)
    {
        State = ProcessingState.Ready;
        NextAttemptAt = nextAttemptAt;
        ErrorMessage = error;
        RetryCount++;
    }
}
