namespace Domain.Assets;

public sealed record SubtitleTrack(
    int TrackIndex,
    string Language,
    string? LanguageDisplay,
    string Format,
    bool IsForced,
    bool IsDefault,
    string? Title);
