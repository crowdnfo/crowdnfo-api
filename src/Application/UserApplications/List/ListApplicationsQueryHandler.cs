using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.UserApplications.List;

internal sealed class ListApplicationsQueryHandler(IApplicationDbContext context)
    : IQueryHandler<ListApplicationsQuery, IReadOnlyList<ApplicationResponse>>
{
    public async Task<Result<IReadOnlyList<ApplicationResponse>>> Handle(
        ListApplicationsQuery query,
        CancellationToken cancellationToken)
    {
        List<ApplicationResponse> applications = await context.UserApplications
            .AsNoTracking()
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new ApplicationResponse(
                a.Id,
                a.UserId,
                context.Users
                    .Where(u => u.Id == a.UserId)
                    .Select(u => u.Username)
                    .FirstOrDefault(),
                a.ApplicationData,
                a.Status,
                a.ReviewComment,
                a.ReviewedAt,
                a.CreatedAt))
            .ToListAsync(cancellationToken);

        return applications;
    }
}
