namespace Domain.Assets;

public sealed class MediaInfoAsset : Asset
{
    private readonly List<AudioTrack> _audioTracks = [];
    private readonly List<SubtitleTrack> _subtitleTracks = [];

    private MediaInfoAsset()
    {
    }

    public override SubmissionType Type => SubmissionType.MediaInfo;

    public string? VideoCodec { get; private set; }

    public string? VideoResolution { get; private set; }

    public long? VideoBitRate { get; private set; }

    public decimal? VideoFrameRate { get; private set; }

    public int? VideoBitDepth { get; private set; }

    public string? HdrFormat { get; private set; }

    public int? DurationSeconds { get; private set; }

    public long? FileSizeBytes { get; private set; }

    public string MediaInfoData { get; private set; }

    public IReadOnlyCollection<AudioTrack> AudioTracks => _audioTracks.AsReadOnly();

    public IReadOnlyCollection<SubtitleTrack> SubtitleTracks => _subtitleTracks.AsReadOnly();

    public static MediaInfoAsset Create(
        string contentHash,
        string mediaInfoData,
        IEnumerable<AudioTrack> audioTracks,
        IEnumerable<SubtitleTrack> subtitleTracks)
    {
        var asset = new MediaInfoAsset
        {
            MediaInfoData = mediaInfoData
        };

        asset._audioTracks.AddRange(audioTracks);
        asset._subtitleTracks.AddRange(subtitleTracks);
        asset.Initialize(contentHash);

        return asset;
    }

    public void SetVideoSummary(
        string? videoCodec,
        string? videoResolution,
        long? videoBitRate,
        decimal? videoFrameRate,
        int? videoBitDepth,
        string? hdrFormat,
        int? durationSeconds,
        long? fileSizeBytes)
    {
        VideoCodec = videoCodec;
        VideoResolution = videoResolution;
        VideoBitRate = videoBitRate;
        VideoFrameRate = videoFrameRate;
        VideoBitDepth = videoBitDepth;
        HdrFormat = hdrFormat;
        DurationSeconds = durationSeconds;
        FileSizeBytes = fileSizeBytes;
    }
}
