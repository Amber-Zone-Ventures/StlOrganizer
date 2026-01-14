using StlOrganizer.Library.SystemAdapters.FileSystem;

namespace StlOrganizer.Library.Decompression;

public class FolderScanner(
    IFileSystem fileSystem,
    IDecompressor decompressor) : IFolderScanner
{
    public async Task ScanAndDecompressAsync(
        string folder,
        CancellationToken cancellationToken = default)
    {
        var files = fileSystem.GetFiles(folder, "*.zip", SearchOption.AllDirectories)
            .ToList();
        
        if (files.Count == 0)
            throw new NoArchivesFoundException();

        foreach (var file in files)
        {
            var fileNameWithoutExtension = fileSystem.GetFileNameWithoutExtension(file);
            var outputPath = fileSystem.CombinePaths(folder, fileNameWithoutExtension);
            await decompressor.DecompressAsync(file, outputPath, cancellationToken);
        }
    }
}
