namespace StlOrganizer.Library.Compression;

using OperationSelection;

public interface ICompressor
{
    Task Compress(
        string source,
        string destination,
        CancellationToken cancellationToken);
}
