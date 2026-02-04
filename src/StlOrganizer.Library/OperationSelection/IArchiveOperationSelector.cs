using StlOrganizer.Library.Compression;

namespace StlOrganizer.Library.OperationSelection;

public interface IArchiveOperationSelector
{
    Task<string> ExecuteOperationAsync(ArchiveOperation operationType,
        string selectedPath,
        IProgress<CompressProgress> progress = null,
        CancellationToken cancellationToken = default);
}
