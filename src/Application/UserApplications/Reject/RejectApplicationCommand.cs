using Application.Abstractions.Messaging;

namespace Application.UserApplications.Reject;

public sealed record RejectApplicationCommand(int ApplicationId, string? Comment) : ICommand;
