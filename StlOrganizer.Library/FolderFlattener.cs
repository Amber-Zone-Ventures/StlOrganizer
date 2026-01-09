namespace StlOrganizer.Library;

public class FolderFlattener
{
    private readonly IDirectoryService directoryService;

    public FolderFlattener() : this(new DirectoryServiceAdapter())
    {
    }

    public FolderFlattener(IDirectoryService directoryService)
    {
        this.directoryService = directoryService;
    }

    public void RemoveNestedFolders(string rootPath)
    {
        if (!directoryService.Exists(rootPath))
        {
            throw new DirectoryNotFoundException($"Directory not found: {rootPath}");
        }

        ProcessDirectory(rootPath);
    }

    private void ProcessDirectory(string directoryPath)
    {
        var subdirectories = directoryService.GetDirectories(directoryPath);

        foreach (var subdirectory in subdirectories)
        {
            // First process nested directories recursively
            ProcessDirectory(subdirectory);

            // Then check if this subdirectory should be flattened
            FlattenIfMatching(directoryPath, subdirectory);
        }
    }

    private void FlattenIfMatching(string parentPath, string childPath)
    {
        var parentName = directoryService.GetDirectoryName(parentPath);
        var childName = directoryService.GetDirectoryName(childPath);

        if (string.Equals(parentName, childName, StringComparison.OrdinalIgnoreCase))
        {
            // Move all contents from child to parent
            directoryService.Move(childPath, parentPath);

            // Delete the now-empty child directory
            directoryService.Delete(childPath, recursive: false);
        }
    }
}
