namespace StlOrganizer.Library;

public sealed class ArchiveOperation : SmartEnum<ArchiveOperation>
{
    public static readonly ArchiveOperation DecompressArchives = new(1, "Decompress files");
    public static readonly ArchiveOperation CompressFolder = new(2, "Compress folder");
    public static readonly ArchiveOperation ExtractImages = new(3, "Extract images");

    private ArchiveOperation(int id, string name) : base(id, name)
    {
    }

    public static implicit operator int(ArchiveOperation operation) => operation.Id;

    public static implicit operator string(ArchiveOperation operation) => operation.Name;
}