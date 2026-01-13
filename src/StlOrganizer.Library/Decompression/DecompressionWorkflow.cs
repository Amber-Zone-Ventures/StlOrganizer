using Serilog;
using StlOrganizer.Library.SystemFileAdapters;

namespace StlOrganizer.Library.Decompression;

public class DecompressionWorkflow(
    IFileDecompressor fileDecompressor,
    IFolderFlattener folderFlattener,
    IFileOperations fileOperations,
    ILogger logger) : IDecompressionWorkflow
{
    public async Task<IEnumerable<string>> ExecuteAsync(string directoryPath, bool deleteOriginalFiles = false, CancellationToken cancellationToken = default)
    {
        logger.Information("Starting decompression workflow for {DirectoryPath}", directoryPath);

        // Step 1: Decompress all files
        var result = await fileDecompressor.ScanAndDecompressAsync(directoryPath, cancellationToken);
        var fileCount = result.ExtractedFiles.Count();
        logger.Information("Decompressed {FileCount} files", fileCount);

        // Step 2: Flatten nested folders
        cancellationToken.ThrowIfCancellationRequested();
        folderFlattener.RemoveNestedFolders(directoryPath);
        logger.Information("Completed folder flattening for {DirectoryPath}", directoryPath);

        // Step 3: Delete original compressed files if requested
        if (deleteOriginalFiles)
        {
            foreach (var compressedFile in result.CompressedFiles)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    fileOperations.DeleteFile(compressedFile);
                    logger.Information("Deleted compressed file {CompressedFile}", compressedFile);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Failed to delete compressed file {CompressedFile}", compressedFile);
                }
            }
        }

        return result.ExtractedFiles;
    }
}
