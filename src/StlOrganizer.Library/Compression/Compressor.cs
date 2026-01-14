using System.IO.Compression;
using Serilog;
using StlOrganizer.Library.SystemAdapters.FileSystem;

namespace StlOrganizer.Library.Compression;

using OperationSelection;

public class Compressor(
    IFileSystem fileSystem,
    IFileOperations fileOperations,
    ILogger logger) : ICompressor
{
    public async Task Compress(
        string source,
        string destination,
        CancellationToken cancellationToken = default)
    {
        await ZipFile.CreateFromDirectoryAsync(
            source,
            destination,
            CompressionLevel.Optimal,
            false,
            cancellationToken );
    }
}
