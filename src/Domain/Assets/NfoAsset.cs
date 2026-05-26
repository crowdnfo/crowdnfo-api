namespace Domain.Assets;

public sealed class NfoAsset : Asset
{
    private readonly List<NfoTag> _tags = [];

    private NfoAsset()
    {
    }

    public override SubmissionType Type => SubmissionType.Nfo;

    public string OriginalFileName { get; private set; }

    public string StoredFileName { get; private set; }

    public long FileSizeBytes { get; private set; }

    public string Text { get; private set; }

    public IReadOnlyCollection<NfoTag> Tags => _tags.AsReadOnly();

    public static NfoAsset Create(
        string contentHash,
        string originalFileName,
        string storedFileName,
        long fileSizeBytes,
        string text)
    {
        var asset = new NfoAsset
        {
            OriginalFileName = originalFileName,
            StoredFileName = storedFileName,
            FileSizeBytes = fileSizeBytes,
            Text = text
        };

        asset.Initialize(contentHash);

        return asset;
    }

    public void SetLlmTags(IEnumerable<NfoTag> tags)
    {
        _tags.Clear();
        _tags.AddRange(tags);
    }
}
