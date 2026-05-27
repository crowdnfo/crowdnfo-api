using Application.Abstractions.Messaging;

namespace Application.UserApplications.Approve;

public sealed record ApproveApplicationCommand(int ApplicationId, string? Comment) : ICommand;
