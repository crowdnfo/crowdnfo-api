using SharedKernel;

namespace Domain.Processing;

/// <summary>
/// Per-step processing status for a release (one row per release + step). The brain of the
/// pipeline lives in code (the step classes); this row is the durable record for observability,
/// idempotency (<see cref="ProcessedInputHash"/>), retry scheduling and admin reset.
/// </summary>
public sealed class ReleaseProcessingStatus : IHasTimestamps
{
    private ReleaseProcessingStatus()
    {
    }

    public Guid Id { get; private set; }

    public int ReleaseId { get; private set; }

    public ProcessingStep Step { get; private set; }

    public ProcessingState State { get; private set; }

    public string? ProcessedInputHash { get; private set; }

    public DateTime? NextAttemptAt { get; private set; }

    public DateTime? LastAttemptedAt { get; private set; }

    public DateTime? LastSuccessfulAt { get; private set; }

    public string? ErrorMessage { get; private set; }

    public int RetryCount { get; private set; }

    /// <summary>Step-specific output (JSON). Write-only observability; never read to make decisions.</summary>
    public string? ResultData { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public static ReleaseProcessingStatus Create(int releaseId, ProcessingStep step) =>
        new()
        {
            Id = Guid.CreateVersion7(),
            ReleaseId = releaseId,
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

    /// <summary>Transient failure: stay runnable but defer until <paramref name="nextAttemptAt"/>.</summary>
    public void ScheduleRetry(DateTime nextAttemptAt, string? error = null)
    {
        State = ProcessingState.Ready;
        NextAttemptAt = nextAttemptAt;
        ErrorMessage = error;
        RetryCount++;
    }
}
