using SharedKernel;

namespace Domain.Community;

public static class ReportErrors
{
    public static Error NotFound(int reportId) => Error.NotFound(
        "Reports.NotFound",
        $"The report with the Id = '{reportId}' was not found");

    public static readonly Error AlreadyResolved = Error.Conflict(
        "Reports.AlreadyResolved",
        "The report has already been resolved");
}
