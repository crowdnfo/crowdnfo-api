namespace Domain.Products;

public sealed record AlternativeTitle(
    string Title,
    string? CountryCode,
    string? LanguageCode);
