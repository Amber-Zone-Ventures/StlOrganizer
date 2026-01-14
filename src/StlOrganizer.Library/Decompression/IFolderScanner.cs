namespace StlOrganizer.Library.Decompression;

using OperationSelection;

public interface IFolderScanner
{
    Task FindAndDecompress(
        string folder,
        IProgress<OrganizerProgress> progress,
        CancellationToken cancellationToken = default);
}
