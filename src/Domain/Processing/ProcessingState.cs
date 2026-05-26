namespace Domain.Processing;

public enum ProcessingState
{
    NotApplicable = 0,
    Ready = 1,
    InProgress = 2,
    Completed = 3,
    Failed = 4
}
