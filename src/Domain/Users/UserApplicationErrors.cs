using SharedKernel;

namespace Domain.Users;

public static class UserApplicationErrors
{
    public static Error NotFound(int applicationId) => Error.NotFound(
        "UserApplications.NotFound",
        $"The application with the Id = '{applicationId}' was not found");

    public static readonly Error NotPending = Error.Conflict(
        "UserApplications.NotPending",
        "The application has already been reviewed");
}
