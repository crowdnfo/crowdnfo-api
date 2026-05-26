namespace Domain.Products;

/// <summary>
/// A per-country release date. The year/month/day allow partial dates from sources that lack a full
/// date; <see cref="ReleaseAttributes"/> is the set of release types for that date (e.g. "theatrical",
/// "dvd", "internet"), as reported by the external source.
/// </summary>
public sealed record ProductReleaseDate(
    string CountryCode,
    DateOnly ReleaseDate,
    int ReleaseYear,
    int? ReleaseMonth,
    int? ReleaseDay,
    IReadOnlyList<string> ReleaseAttributes)
{
    public bool Equals(ProductReleaseDate? other) =>
        other is not null
        && CountryCode == other.CountryCode
        && ReleaseDate == other.ReleaseDate
        && ReleaseYear == other.ReleaseYear
        && ReleaseMonth == other.ReleaseMonth
        && ReleaseDay == other.ReleaseDay
        && ReleaseAttributes.SequenceEqual(other.ReleaseAttributes);

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(CountryCode);
        hashCode.Add(ReleaseDate);
        hashCode.Add(ReleaseYear);
        hashCode.Add(ReleaseMonth);
        hashCode.Add(ReleaseDay);
        foreach (string releaseAttribute in ReleaseAttributes)
        {
            hashCode.Add(releaseAttribute);
        }

        return hashCode.ToHashCode();
    }
}
