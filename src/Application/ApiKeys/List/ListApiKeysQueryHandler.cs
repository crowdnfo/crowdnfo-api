using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.ApiKeys.List;

internal sealed class ListApiKeysQueryHandler(
    IApplicationDbContext context,
    IUserContext userContext) : IQueryHandler<ListApiKeysQuery, IReadOnlyList<ApiKeyResponse>>
{
    public async Task<Result<IReadOnlyList<ApiKeyResponse>>> Handle(
        ListApiKeysQuery query,
        CancellationToken cancellationToken)
    {
        List<ApiKeyResponse> keys = await context.ApiKeys
            .AsNoTracking()
            .Where(k => k.UserId == userContext.UserId)
            .OrderByDescending(k => k.CreatedAt)
            .Select(k => new ApiKeyResponse(k.Id, k.Name, k.CreatedAt))
            .ToListAsync(cancellationToken);

        return keys;
    }
}
