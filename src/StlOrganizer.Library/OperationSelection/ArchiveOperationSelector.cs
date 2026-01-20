using Serilog;
using StlOrganizer.Library.Compression;
using StlOrganizer.Library.Decompression;
using StlOrganizer.Library.ImageProcessing;

namespace StlOrganizer.Library.OperationSelection;

public class ArchiveOperationSelector(
    IDecompressionWorkflow decompressionWorkflow,
    ICompressor compressor,
    IImageOrganizer imageOrganizer,
    ILogger logger) : IArchiveOperationSelector
{
    public async Task<string> ExecuteOperationAsync(
        ArchiveOperation operationType,
        string selectedPath,  
        CancellationToken cancellationToken)
    {
        return operationType switch
        {
            _ when operationType == ArchiveOperation.DecompressArchives => await ExecuteFileDecompressorAsync(selectedPath, cancellationToken),
            _ when operationType == ArchiveOperation.CompressFolder => await ExecuteFolderCompressorAsync(selectedPath, cancellationToken),
            _ when operationType == ArchiveOperation.ExtractImages => await ExecuteImageOrganizerAsync(selectedPath, cancellationToken),
            _ => throw new ArgumentException($"Unknown operation type: {operationType.Name}")
        };
    }

    private async Task<string> ExecuteFileDecompressorAsync(string selectedPath, CancellationToken cancellationToken)
    {
        await decompressionWorkflow.Execute(selectedPath, new Progress<OrganizerProgress>(), cancellationToken);
        return "Successfully extracted file(s) and flattened folders.";
    }

    private async Task<string> ExecuteFolderCompressorAsync(string source, CancellationToken cancellationToken)
    {
        var outputPath = source + ".zip";
        
        await compressor.Compress(
            source, 
            source + ".zip",
            cancellationToken);
        
        return $"Successfully created archive: {outputPath}";
    }

    private async Task<string> ExecuteImageOrganizerAsync(string selectedPath, CancellationToken cancellationToken)
    {
        var copiedCount = await imageOrganizer.OrganizeImagesAsync(selectedPath, cancellationToken);
        logger.Information("ImageOrganizer copied {CopiedCount} images", copiedCount);
        return $"Successfully copied {copiedCount} image(s) to Images folder.";
    }
}