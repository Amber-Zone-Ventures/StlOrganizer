namespace StlOrganizer.Library.OperationSelection;

public interface IArchiveOperationSelector
{
    Task<string> ExecuteOperationAsync(ArchiveOperation operationType,
        string selectedPath,
        CancellationToken cancellationToken);
}
