namespace Domain.Processing;

public enum ProcessingStep
{
    NfoAnalysis = 0,
    ReleaseTitleAnalysis = 1,
    LlmAnalysis = 2,
    ImdbSearch = 3,
    ImdbBatch = 4,
    ImdbDetail = 5,
    MediathekArr = 6
}
