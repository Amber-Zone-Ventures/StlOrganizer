using StlOrganizer.Library.SystemAdapters;

namespace StlOrganizer.Gui.OperationSelection;

public sealed class ArchiveOperation(int id, string name) : SmartEnum<ArchiveOperation>(id, name)
{
    public static readonly ArchiveOperation DecompressArchives = new(1, "Decompress archives");
    public static readonly ArchiveOperation CompressFolder = new(2, "Compress folder");
    public static readonly ArchiveOperation ExtractImages = new(3, "Extract images");
}
