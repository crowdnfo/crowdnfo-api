namespace Domain.Products;

/// <summary>
/// A product's external id. A child entity (not owned) so it can be queried directly to find the
/// owning product for the pipeline's find-or-create.
/// </summary>
public sealed class ProductExternalId
{
    private ProductExternalId()
    {
    }

    public int Id { get; private set; }

    public int ProductId { get; private set; }

    public ExternalIdSource Source { get; private set; }

    public string Value { get; private set; }

    internal static ProductExternalId Create(ExternalIdSource source, string value) =>
        new()
        {
            Source = source,
            Value = Normalize(source, value)
        };

    internal void UpdateValue(string value) => Value = Normalize(Source, value);

    private static string Normalize(ExternalIdSource source, string value) =>
        source == ExternalIdSource.Imdb ? ImdbIdNormalizer.Normalize(value) : value.Trim();
}
