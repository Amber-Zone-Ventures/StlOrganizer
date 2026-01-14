namespace StlOrganizer.Library.Decompression;

public interface IFolderFlattener
{
    Task RemoveNestedFolders(string rootPath, CancellationToken cancellationToken);
}
