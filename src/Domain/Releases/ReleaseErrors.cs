using SharedKernel;

namespace Domain.Releases;

public static class ReleaseErrors
{
    public static Error NotFound(int releaseId) => Error.NotFound(
        "Releases.NotFound",
        $"The release with the Id = '{releaseId}' was not found");

    public static readonly Error NameNotUnique = Error.Conflict(
        "Releases.NameNotUnique",
        "A release with the same canonical name already exists");
}
