namespace StlOrganizer.Library;

public interface IArchiveOperationSelector
{
    Task<string> ExecuteOperationAsync(ArchiveOperation operationType, string directoryPath, CancellationToken cancellationToken);
}
