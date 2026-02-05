namespace StlOrganizer.Library.Decompression;

public interface IDecompressionWorkflow
{
    Task Execute(
        string directoryPath,
        IProgress<DecompressionProgress> progress,
        CancellationToken cancellationToken = default);
}
