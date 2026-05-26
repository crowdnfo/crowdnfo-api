using SharedKernel;

namespace Domain.Community;

public static class CommentErrors
{
    public static Error NotFound(int commentId) => Error.NotFound(
        "Comments.NotFound",
        $"The comment with the Id = '{commentId}' was not found");
}
