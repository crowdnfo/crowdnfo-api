namespace Domain.Products;

internal static class ImdbIdNormalizer
{
    internal static string Normalize(string imdbId) => imdbId.Trim().ToLowerInvariant();
}
