namespace StlOrganizer.Library.Decompression;

public interface IFolderScanner
{
    Task FindAndDecompress(
        string folder,
        IProgress<DecompressionProgress> progress,
        CancellationToken cancellationToken = default);
}
