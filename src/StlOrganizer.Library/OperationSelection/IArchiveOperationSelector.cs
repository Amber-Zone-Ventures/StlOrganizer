namespace StlOrganizer.Library.OperationSelection;

public interface IArchiveOperationSelector
{
    Task<string> ExecuteOperationAsync(ArchiveOperation operationType, string directoryPath, CancellationToken cancellationToken);
}
