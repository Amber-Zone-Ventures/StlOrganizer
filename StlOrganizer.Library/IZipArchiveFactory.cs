using System.IO.Compression;

namespace StlOrganizer.Library;

public interface IZipArchiveFactory
{
    IZipArchive Open(string archiveFileName, ZipArchiveMode mode);
}
