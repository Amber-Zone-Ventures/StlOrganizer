using System.IO.Compression;

namespace StlOrganizer.Library;

public interface IZipArchive : IDisposable
{
    void CreateEntryFromFile(string sourceFileName, string entryName, CompressionLevel compressionLevel);
}
