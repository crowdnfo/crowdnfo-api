namespace Domain.Assets;

public sealed record AudioTrack(
    int TrackIndex,
    string Language,
    string? LanguageDisplay,
    string Codec,
    int? Channels,
    int? BitRate,
    int? DurationSeconds,
    bool IsDefault,
    string? Title);
