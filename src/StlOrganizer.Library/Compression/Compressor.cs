using System.IO.Compression;

namespace StlOrganizer.Library.Compression;

public class Compressor : ICompressor
{
    public Task Compress(string source, string destination, IProgress<CompressProgress> progress, CancellationToken cancellationToken = default)
    {
        return ZipDirectoryAsync(source, destination, CompressionLevel.Optimal, progress, cancellationToken);
    }
    
    private async Task ZipDirectoryAsync(
        string sourceDir,
        string zipPath,
        CompressionLevel compressionLevel = CompressionLevel.Optimal,
        IProgress<CompressProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var files = Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories);

        var totalBytes = files.Sum(f => new FileInfo(f).Length);

        long processedBytes = 0;

        // Overwrite if it exists
        await using var zipFileStream = new FileStream(zipPath, FileMode.Create, FileAccess.Write, FileShare.None);
        await using var archive = new ZipArchive(zipFileStream, ZipArchiveMode.Create, false);

        var buffer = new byte[81920];

        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var entryName = Path.GetRelativePath(sourceDir, file).Replace('\\', '/');

            // Create ZIP entry and write the file into it
            var entry = archive.CreateEntry(entryName, compressionLevel);

            // Preserve last write time (optional but nice)
            entry.LastWriteTime = File.GetLastWriteTimeUtc(file);

            await using var input = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
            await using var entryStream = await entry.OpenAsync(cancellationToken);

            int bytesRead;
            while ((bytesRead = await input.ReadAsync(buffer, cancellationToken)) > 0)
            {
                await entryStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);

                processedBytes += bytesRead;
                if (totalBytes > 0)
                    progress?.Report(
                        new CompressProgress(
                            (int)((double)processedBytes / totalBytes * 100),
                            Path.GetFileName(file)));
            }
        }

        // Disposing ZipArchive writes the central directory; no need to call Finish().
    }
}
