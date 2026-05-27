using SharedKernel;

namespace Domain.Users;

public static class RoleErrors
{
    public static Error NotFound(int roleId) => Error.NotFound(
        "Roles.NotFound",
        $"The role with the Id = '{roleId}' was not found");

    public static Error NotFoundByName(string name) => Error.NotFound(
        "Roles.NotFoundByName",
        $"The role '{name}' was not found");
}
