using SharedKernel;

namespace Domain.Users;

public static class ApiKeyErrors
{
    public static Error NotFound(Guid apiKeyId) => Error.NotFound(
        "ApiKeys.NotFound",
        $"The API key with the Id = '{apiKeyId}' was not found");

    public static readonly Error NameNotUnique = Error.Conflict(
        "ApiKeys.NameNotUnique",
        "An API key with the same name already exists");
}
