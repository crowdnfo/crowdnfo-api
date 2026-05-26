namespace Domain.Assets;

public sealed class FileListAsset : Asset
{
    private readonly List<FileListEntry> _entries = [];

    private FileListAsset()
    {
    }

    public override SubmissionType Type => SubmissionType.FileList;

    public IReadOnlyCollection<FileListEntry> Entries => _entries.AsReadOnly();

    public static FileListAsset Create(
        string contentHash,
        IEnumerable<FileListEntry> entries)
    {
        var asset = new FileListAsset();

        asset._entries.AddRange(entries);
        asset.Initialize(contentHash);

        return asset;
    }
}
