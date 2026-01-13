namespace StlOrganizer.Library.Decompression;

public interface IFileDecompressor
{
    Task<DecompressionResult> ScanAndDecompressAsync(string directoryPath, CancellationToken cancellationToken = default);
}
