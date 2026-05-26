namespace SharedKernel;

/// <summary>
/// Opt-in marker for entities whose audit timestamps are stamped automatically by an
/// Infrastructure <c>SaveChangesInterceptor</c>. Properties are get-only here so the Domain
/// stays free of persistence concerns; the interceptor writes them via the EF change tracker.
/// </summary>
public interface IHasTimestamps
{
    DateTime CreatedAt { get; }

    DateTime UpdatedAt { get; }
}
