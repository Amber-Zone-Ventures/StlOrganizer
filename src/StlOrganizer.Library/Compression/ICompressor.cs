namespace StlOrganizer.Library.Compression;

public interface ICompressor
{
    Task Compress(
        string source,
        string destination,
        IProgress<CompressProgress> progress,
        CancellationToken cancellationToken = default);
}
