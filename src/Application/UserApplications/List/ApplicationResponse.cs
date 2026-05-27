using Domain.Users;

namespace Application.UserApplications.List;

public sealed record ApplicationResponse(
    int Id,
    Guid? UserId,
    string? Username,
    string ApplicationData,
    ApplicationStatus Status,
    string? ReviewComment,
    DateTime? ReviewedAt,
    DateTime CreatedAt);
