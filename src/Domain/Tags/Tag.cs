namespace Domain.Tags;

/// <summary>
/// Origin is encoded by the value object carrying the tag (ReleaseTag vs NfoTag), not a field:
/// name-derived tags (0..99) live on the release, LLM-derived tags (100+) on the NFO asset.
/// </summary>
public enum Tag
{
    // From the release name (pattern-based)
    Proper = 0,
    ReadNfo = 1,
    Repack = 2,
    Internal = 3,

    // From LLM analysis of the NFO
    HasNotes = 100,
    SceneDrama = 101,
    GroupNews = 102,
    Technical = 103
}
