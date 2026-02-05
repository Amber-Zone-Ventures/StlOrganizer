using StlOrganizer.Library.SystemAdapters.FileSystem;

namespace StlOrganizer.Library.Decompression;

public class FolderScanner(
    IFileSystem fileSystem,
    IDecompressor decompressor) : IFolderScanner
{
    public async Task FindAndDecompress(
        string folder,
        IProgress<DecompressionProgress> progress,
        CancellationToken cancellationToken = default)
    {
        const string extensionToFind = "*.zip";

        var archives = fileSystem.GetFiles(
                folder, extensionToFind, SearchOption.AllDirectories)
            .ToList();

        if (archives.Count == 0)
            throw new NoArchivesFoundException();

        var count = 0;

        foreach (var file in archives)
        {
            count++;
            progress.Report(new DecompressionProgress
            {
                Message = $"Decompressing {file}.",
                Progress = (int)((double)count / archives.Count * 100)
            });

            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);
            var outputPath = Path.Combine(folder, fileNameWithoutExtension);
            await decompressor.DecompressAsync(file, outputPath, cancellationToken);
        }
    }
}
