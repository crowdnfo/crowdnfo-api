using SharedKernel;

namespace Domain.Assets;

public static class SubmissionErrors
{
    public static Error AssetNotFound(Guid assetId) => Error.NotFound(
        "Submissions.AssetNotFound",
        $"The asset with the Id = '{assetId}' was not found");

    public static readonly Error AlreadySubmittedByUser = Error.Conflict(
        "Submissions.AlreadySubmittedByUser",
        "This user has already submitted this content");
}
